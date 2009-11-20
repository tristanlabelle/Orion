using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Commandment
{
    /// <summary>
    /// Handles the selection of <see cref="Unit"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public const int MaxSelectedUnits = 16;

        private Unit hoveredUnit;
        private readonly List<Unit> selectedUnits = new List<Unit>();
        private readonly Faction faction;
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
        }
        #endregion

        #region Events
        public event GenericEventHandler<SelectionManager> SelectionCleared;
        public event GenericEventHandler<SelectionManager> SelectionChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence of units currently selected.
        /// </summary>
        public IEnumerable<Unit> SelectedUnits
        {
            get { return selectedUnits; }
        }

        public Unit HoveredUnit
        {
            get { return hoveredUnit; }
            set { hoveredUnit = value; }
        }
        #endregion

        #region Methods
        public void SelectUnit(Unit unit)
        {
            ClearSelection();
            AppendToSelection(unit);
        }

        public void SelectUnits(IEnumerable<Unit> units)
        {
            ClearSelection();
            AppendToSelection(units);
        }

        public void AppendToSelection(Unit unit)
        {
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                OnSelectionChange();
            }
        }

        public void AppendToSelection(IEnumerable<Unit> units)
        {
            selectedUnits.AddRange(units.Except(selectedUnits));
            OnSelectionChange();
        }

        public void ClearSelection()
        {
            selectedUnits.Clear();
            GenericEventHandler<SelectionManager> selectionCleared = SelectionCleared;
            if (selectionCleared != null) selectionCleared(this);
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that an entity died.
        /// </summary>
        /// <param name="source">The source EntityRegistry</param>
        /// <param name="entity">The entity that died</param>
        public void EntityDied(EntityRegistry source, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit != null && selectedUnits.Contains(unit))
            {
                selectedUnits.Remove(unit);
                OnSelectionChange();
            }
        }

        private void OnSelectionChange()
        {
            selectedUnits.Sort((a, b) => a.Type.Handle.Value.CompareTo(b.Type.Handle.Value));
            if (selectedUnits.Count > MaxSelectedUnits)
                selectedUnits.RemoveRange(MaxSelectedUnits, selectedUnits.Count - MaxSelectedUnits);
            GenericEventHandler<SelectionManager> handler = SelectionChanged;
            if (handler != null) handler(this);
        }
        #endregion
    }
}
