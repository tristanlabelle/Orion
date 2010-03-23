using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Handles the selection of <see cref="Unit"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public static readonly int SelectionLimit = 24;
        public static readonly int SelectionGroupCount = 10;
        public static readonly float NearbySelectionRadius = 10;

        private readonly Faction faction;
        private readonly List<Unit> selectedUnits = new List<Unit>();
        private readonly HashSet<Unit>[] selectionGroups;
        private UnitType selectedUnitType;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SelectionManager"/> from the <see cref="Faction"/>
        /// which is being controlled.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> used for now.</param>
        public SelectionManager(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
            this.faction.World.Updated += OnWorldUpdated;
            this.faction.World.Entities.Removed += OnEntityRemoved;
            this.selectionGroups = new HashSet<Unit>[SelectionGroupCount];
            for (int i = 0; i < this.selectionGroups.Length; ++i)
                this.selectionGroups[i] = new HashSet<Unit>();
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the contents of the selection has changed.
        /// </summary>
        public event Action<SelectionManager> SelectionChanged;

        /// <summary>
        /// Raised when the currently selected unit type has changed.
        /// </summary>
        public event Action<SelectionManager> SelectedUnitTypeChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the faction towards which this <see cref="SelectionManager"/> has a bias.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }

        /// <summary>
        /// Gets the sequence of units currently selected.
        /// </summary>
        public IEnumerable<Unit> SelectedUnits
        {
            get { return selectedUnits; }
        }

        /// <summary>
        /// Gets the number of units which are currently selected.
        /// </summary>
        public int SelectedUnitCount
        {
            get { return selectedUnits.Count; }
        }

        /// <summary>
        /// Gets a value indicating if no units are currently selected.
        /// </summary>
        public bool IsSelectionEmpty
        {
            get { return selectedUnits.Count == 0; }
        }

        /// <summary>
        /// Gets a value indicating if the maximum number of units are currently selected.
        /// </summary>
        public bool IsSelectionFull
        {
            get { return selectedUnits.Count == SelectionLimit; }
        }

        /// <summary>
        /// Gets the type of unit which has the focus within the selection.
        /// This is never null when the selection isn't empty.
        /// </summary>
        public UnitType SelectedUnitType
        {
            get { return selectedUnitType; }
            set
            {
                if (value == selectedUnitType) return;
                if (IsSelectionEmpty) throw new ArgumentException("The selected unit type cannot be null when there's a selection.");
                if (selectedUnits.None(unit => unit.Type == value))
                    throw new ArgumentException("The selected unit type must be the type of a unit in the selection.");
                selectedUnitType = value;
                SelectedUnitTypeChanged.Raise(this);
            }
        }

        /// <summary>
        /// Gets the unit that leads the selection.
        /// </summary>
        public Unit LeadingUnit
        {
            get { return IsSelectionEmpty ? null : selectedUnits.First(unit => unit.Type == selectedUnitType); }
        }

        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        #region Selection modification
        /// <summary>
        /// Clears the selection and selects a single unit.
        /// </summary>
        /// <param name="unit">The unit to be selected.</param>
        public void SetSelection(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            selectedUnits.Clear();
            if (IsSelectable(unit)) selectedUnits.Add(unit);
            ResetSelectedUnitType();
            SelectionChanged.Raise(this);
        }

        /// <summary>
        /// Clears the selection and adds some unit to it.
        /// </summary>
        /// <param name="units">The units to be added.</param>
        public void SetSelection(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            selectedUnits.Clear();
            foreach (Unit unit in units)
                if (IsSelectable(unit))
                    selectedUnits.Add(unit);

            SortSelection();
            ResetSelectedUnitType();
            SelectionChanged.Raise(this);
        }

        /// <summary>
        /// Clears the selection and selects units in a radius with a given type.
        /// </summary>
        /// <param name="position">The position from which to check for units.</param>
        /// <param name="unitType">The type of the units to be selected.</param>
        public void SelectNearbyUnitsOfType(Vector2 position, UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");

            IEnumerable<Unit> selectedUnits = faction.World.Entities
                .OfType<Unit>()
                .Where(unit => (unit.Center - position).LengthSquared <= NearbySelectionRadius * NearbySelectionRadius
                    && unit.Type == unitType
                    && unit.Faction == faction);
            
            SetSelection(selectedUnits);
        }

        /// <summary>
        /// Attempts to add a given unit to the selection.
        /// </summary>
        /// <param name="unit">The unit to be added.</param>
        public void AddToSelection(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            if (!IsSelectable(unit)) return;

            selectedUnits.Add(unit);
            SortSelection();
            SelectionChanged.Raise(this);
        }

        /// <summary>
        /// Adds numerous units to the selection.
        /// </summary>
        /// <param name="units">The units to be added.</param>
        public void AddToSelection(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            bool wasUnitAdded = false;
            foreach (Unit unit in units)
            {
                if (!IsSelectable(unit)) continue;
                
                selectedUnits.Add(unit);
                wasUnitAdded = true;
            }

            if (wasUnitAdded)
            {
                SortSelection();
                SelectionChanged.Raise(this);
            }
        }

        /// <summary>
        /// Toggles the selection of a single unit.
        /// </summary>
        /// <param name="unit">The unit to be added or removed.</param>
        public void ToggleSelection(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            if (selectedUnits.Contains(unit))
                RemoveFromSelection(unit);
            else
                AddToSelection(unit);
        }

        /// <summary>
        /// Selects the units that intersect a rectangle.
        /// </summary>
        /// <param name="rectangleStart">The start of the rectangle. Units closer to this are selected first.</param>
        /// <param name="rectangleEnd">The end of the rectangle.</param>
        public void SelectInRectangle(Vector2 rectangleStart, Vector2 rectangleEnd)
        {
            SelectInRectangle(rectangleStart, rectangleEnd, false);
        }

        /// <summary>
        /// Adds units intersecting a rectangle to the selection.
        /// </summary>
        /// <param name="rectangleStart">The start of the rectangle. Units closer to this are selected first.</param>
        /// <param name="rectangleEnd">The end of the rectangle.</param>
        public void AddRectangleToSelection(Vector2 rectangleStart, Vector2 rectangleEnd)
        {
            SelectInRectangle(rectangleStart, rectangleEnd, true);
        }

        private void SelectInRectangle(Vector2 rectangleStart, Vector2 rectangleEnd, bool add)
        {
            Rectangle rectangle = Rectangle.FromPoints(rectangleStart, rectangleEnd);

            List<Unit> units = faction.World.Entities
                .OfType<Unit>()
                .Where(unit => Rectangle.Intersects(rectangle, unit.BoundingRectangle))
                .OrderBy(unit => (unit.Center - rectangleStart).LengthSquared)
                .ToList();

            // Filter out factions
            bool containsUnitsFromThisFaction = units.Any(unit => unit.Faction == faction);
            if (containsUnitsFromThisFaction)
                units.RemoveAll(unit => unit.Faction != faction);
            else if (units.Count > 1)
                units.RemoveRange(1, units.Count - 1);

            // Filter out buildings
            bool containsNonBuildingUnits = units.Any(unit => !unit.Type.IsBuilding);
            if (containsNonBuildingUnits) units.RemoveAll(unit => unit.Type.IsBuilding);

            if (add) AddToSelection(units);
            else SetSelection(units);
        }

        /// <summary>
        /// Removes a unit from the current selection.
        /// </summary>
        /// <param name="unit">The unit to be removed from the selection.</param>
        public void RemoveFromSelection(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            if (!selectedUnits.Remove(unit)) return;

            SortSelection();

            if (unit.Type == selectedUnitType)
                UpdateSelectedUnitType();

            SelectionChanged.Raise(this);
        }

        /// <summary>
        /// Removes all units from the selection.
        /// </summary>
        public void ClearSelection()
        {
            if (selectedUnits.Count == 0) return;
            selectedUnits.Clear();
            selectedUnitType = null;
            SelectedUnitTypeChanged.Raise(this);
            SelectionChanged.Raise(this);
        }

        private bool IsSelectable(Unit unit)
        {
            return !IsSelectionFull
                && !selectedUnits.Contains(unit)
                && unit.IsAlive
                && faction.CanSee(unit);
        }

        private void SortSelection()
        {
            // Sort first by unit types, so that the groups with the most units come first,
            // and then by unit handle.
            var selectedUnitTypes = selectedUnits.GroupBy(unit => unit.Type)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .ToList();

            selectedUnits.Sort((a, b) =>
                {
                    int comparison = selectedUnitTypes.IndexOf(a.Type).CompareTo(selectedUnitTypes.IndexOf(b.Type));
                    if (comparison == 0) comparison = a.Handle.Value.CompareTo(b.Handle.Value);
                    return comparison;
                });
        }
        #endregion

        #region Selected Unit Type
        /// <summary>
        /// Resets the selected unit type to its default value according to the current selection.
        /// </summary>
        public void ResetSelectedUnitType()
        {
            UnitType newSelectedUnitType = selectedUnits.Select(unit => unit.Type).FirstOrDefault();
            if (newSelectedUnitType == selectedUnitType) return;

            selectedUnitType = newSelectedUnitType;
            SelectedUnitTypeChanged.Raise(this);
        }

        private void UpdateSelectedUnitType()
        {
            if (selectedUnits.Any(u => u.Type == selectedUnitType)) return;

            Unit newSelectionLeader = selectedUnits.FirstOrDefault();
            selectedUnitType = newSelectionLeader == null ? null : newSelectionLeader.Type;
            SelectedUnitTypeChanged.Raise(this);
        }
        #endregion

        #region Groups
        /// <summary>
        /// Copies the current selection to a selection group.
        /// </summary>
        /// <param name="index">The index of the selection group to be filled.</param>
        public void SaveSelectionGroup(int index)
        {
            ValidateGroupIndex(index);

            var selectionGroup = selectionGroups[index];
            selectionGroup.Clear();
            selectionGroup.UnionWith(selectedUnits);
        }

        /// <summary>
        /// Loads a selection group if it isn't empty.
        /// </summary>
        /// <param name="index">The index of the selection group to be loaded.</param>
        /// <returns>True is the selection group was loaded, false if it was empty.</returns>
        public bool TryLoadSelectionGroup(int index)
        {
            ValidateGroupIndex(index);

            var selectionGroup = selectionGroups[index];
            if (selectionGroup.Count == 0) return false;

            selectionGroup.RemoveWhere(unit => !faction.CanSee(unit));

            SetSelection(selectionGroup);

            return true;
        }

        /// <summary>
        /// Gets a value indicating if a selection group contains a unit.
        /// </summary>
        /// <param name="unit">The unit to be found.</param>
        /// <param name="index">The index of the selection group in which to look.</param>
        /// <returns>True if the unit is in that selection group, false if not.</returns>
        public bool IsInSelectionGroup(Unit unit, int index)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureWithin(index, 0, SelectionGroupCount - 1, "index");

            return selectionGroups[index].Contains(unit);
        }

        private void ValidateGroupIndex(int index)
        {
            if (index < 0 || index >= selectionGroups.Length)
                throw new ArgumentOutOfRangeException("Selection group index out of range.");
        }
        #endregion

        private void OnWorldUpdated(World arg1, SimulationStep arg2)
        {
            RemoveHiddenUnitsFromSelection();
        }

        private void RemoveHiddenUnitsFromSelection()
        {
            bool wasUnitRemoved = false;
            for (int i = selectedUnits.Count - 1; i >= 0; --i)
            {
                Unit unit = selectedUnits[i];
                if (!faction.CanSee(unit))
                {
                    selectedUnits.RemoveAt(i);
                    wasUnitRemoved = true;
                }
            }

            if (wasUnitRemoved)
            {
                UpdateSelectedUnitType();
                SelectionChanged.Raise(this);
            }
        }

        private void OnEntityRemoved(EntityManager source, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            foreach (var selectionGroup in selectionGroups)
                selectionGroup.Remove(unit);

            RemoveFromSelection(unit);
        }
        #endregion
    }
}
