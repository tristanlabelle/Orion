using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic;

namespace Orion.Matchmaking
{
    /// <summary>
    /// Handles the selection of <see cref="Unit"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public static readonly int SelectionLimit = 24;
        public static readonly int SelectionGroupCount = 10;

        private readonly Faction faction;
        private readonly List<Unit> selectedUnits = new List<Unit>();
        private readonly HashSet<Unit>[] selectionGroups;
        private Unit hoveredUnit;
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
        /// Gets the number of units that are currently selected.
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

        public Unit HoveredUnit
        {
            get { return hoveredUnit; }
            set { hoveredUnit = value; }
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
        public void SelectUnit(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");

            selectedUnits.Clear();
            if (IsSelectable(unit)) selectedUnits.Add(unit);
            SelectionChanged.Raise(this);
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
        /// Clears the selection and adds some unit to it.
        /// </summary>
        /// <param name="units">The units to be added.</param>
        public void SelectUnits(IEnumerable<Unit> units)
        {
            Argument.EnsureNotNull(units, "units");

            selectedUnits.Clear();
            foreach (Unit unit in units)
                if (IsSelectable(unit))
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
        /// Removes all units from the selection.
        /// </summary>
        public void ClearSelection()
        {
            if (selectedUnits.Count == 0) return;
            selectedUnits.Clear();
            SelectionChanged.Raise(this);
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

            selectedUnits.Clear();
            selectedUnits.AddRange(selectionGroup);

            SortSelection();
            SelectionChanged.Raise(this);

            return true;
        }

        private void ValidateGroupIndex(int index)
        {
            if (index < 0 || index >= selectionGroups.Length)
                throw new ArgumentOutOfRangeException("Selection group index out of range.");
        }
        #endregion

        private void OnEntityRemoved(EntityManager source, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (hoveredUnit == unit) hoveredUnit = null;

            foreach (var selectionGroup in selectionGroups)
                selectionGroup.Remove(unit);

            if (selectedUnits.Remove(unit))
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
            selectedUnits.Sort((a, b) => a.Type.Handle.Value.CompareTo(b.Type.Handle.Value));
        }
        #endregion
    }
}
