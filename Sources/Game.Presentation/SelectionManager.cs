using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using System.Diagnostics;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Handles the selection of <see cref="Unit"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public static readonly int SelectionLimit = int.MaxValue;
        public static readonly int GroupCount = 10;

        private readonly Faction faction;
        private readonly Selection selection;
        private readonly HashSet<Unit>[] groups;
        private UnitType focusedUnitType;
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
            this.faction.World.EntityRemoved += OnEntityRemoved;

            this.selection = new Selection(faction, SelectionLimit);
            this.selection.Changed += OnSelectionChanged;

            this.groups = new HashSet<Unit>[GroupCount];
            for (int i = 0; i < this.groups.Length; ++i)
                this.groups[i] = new HashSet<Unit>();
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the unit type that is currently focused has changed.
        /// </summary>
        public event Action<SelectionManager> FocusedUnitTypeChanged;
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
        /// Gets the selection.
        /// </summary>
        public Selection Selection
        {
            get { return selection; }
        }

        /// <summary>
        /// Gets the type of unit which currently has the focus.
        /// </summary>
        public UnitType FocusedUnitType
        {
            get { return focusedUnitType; }
            set
            {
                if (value == focusedUnitType) return;

                if (value == null)
                {
                    if (selection.Type == SelectionType.Units)
                        throw new ArgumentException("Cannot set the focused unit type to null when the selection contains units.");
                    Debug.Assert(focusedUnitType == value);
                    return;
                }

                if (selection.Type != SelectionType.Units)
                    throw new ArgumentException("Cannot change the focused unit type when the selection contains no units.");

                if (selection.Units.None(unit => unit.Type == value))
                    throw new ArgumentException("The focused unit type must be of a type present in the selection.");

                focusedUnitType = value;
            }
        }

        /// <summary>
        /// Gets the unit that has the focus.
        /// </summary>
        public Unit FocusedUnit
        {
            get { return selection.Units.FirstOrDefault(unit => unit.Type == focusedUnitType); }
        }

        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        #region Selected Unit Type
        /// <summary>
        /// Resets the focused unit type to its default value according to the current selection.
        /// </summary>
        public void ResetFocusedUnitType()
        {
            UnitType newSelectedUnitType = selection.Units
                .Select(unit => unit.Type)
                .FirstOrDefault();
            if (newSelectedUnitType == focusedUnitType) return;

            focusedUnitType = newSelectedUnitType;
            FocusedUnitTypeChanged.Raise(this);
        }

        private void UpdateFocusedUnitType()
        {
            bool isStillValid = selection.Units.Any(u => u.Type == focusedUnitType);
            if (isStillValid) return;

            if (selection.Type == SelectionType.Units)
                focusedUnitType = selection.Units.GroupBy(u => u.Type).WithMax(group => group.Count()).Key;
            else
                focusedUnitType = null;

            FocusedUnitTypeChanged.Raise(this);
        }

        private void OnSelectionChanged(Selection sender)
        {
            UpdateFocusedUnitType();
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

            if (Selection.Type != SelectionType.Units) return;

            var group = groups[index];
            group.Clear();
            group.UnionWith(Selection.Units);
        }

        /// <summary>
        /// Loads a selection group if it isn't empty.
        /// </summary>
        /// <param name="index">The index of the selection group to be loaded.</param>
        /// <returns>True is the selection group was loaded, false if it was empty.</returns>
        public bool TryLoadSelectionGroup(int index)
        {
            ValidateGroupIndex(index);

            var group = groups[index];
            if (group.Count == 0) return false;

            group.RemoveWhere(unit => !faction.CanSee(unit));

            Selection.Set(group);

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
            ValidateGroupIndex(index);

            return groups[index].Contains(unit);
        }

        private void ValidateGroupIndex(int index)
        {
            if (index < 0 || index >= groups.Length)
                throw new ArgumentOutOfRangeException("Selection group index out of range.");
        }
        #endregion

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            foreach (var group in groups)
                group.Remove(unit);
        }
        #endregion
    }
}
