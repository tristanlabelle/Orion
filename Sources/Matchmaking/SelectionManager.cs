using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Matchmaking
{
    /// <summary>
    /// Handles the selection of <see cref="Unit"/>s using the mouse and keyboard.
    /// </summary>
    public sealed class SelectionManager
    {
        #region Fields
        public const int MaxSelectedUnits = 24;

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

        private void RaiseSelectionCleared()
        {
            var handler = SelectionCleared;
            if (handler != null) handler(this);
        }

        private void RaiseSelectionChanged()
        {
            GenericEventHandler<SelectionManager> handler = SelectionChanged;
            if (handler != null) handler(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the local faction towards which this <see cref="SelectionManager"/> has a bias.
        /// </summary>
        public Faction LocalFaction
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
                if (faction.GetTileVisibility((Point)unit.Position) == TileVisibility.Visible)
                {
                    selectedUnits.Add(unit);
                    UpdateSelection();
                }
            }
        }

        public void AppendToSelection(IEnumerable<Unit> units)
        {
            foreach (Unit unit in units)
            {
                Point unitPosition = (Point)unit.Position;
                if (faction.GetTileVisibility(unitPosition) == TileVisibility.Visible)
                {
                    if (!selectedUnits.Contains(unit))
                        selectedUnits.Add(unit);
                }
            }
            UpdateSelection();
        }

        public void ClearSelection()
        {
            selectedUnits.Clear();
            RaiseSelectionCleared();
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that an entity died.
        /// </summary>
        /// <param name="source">The source EntityRegistry</param>
        /// <param name="entity">The entity that died</param>
        public void EntityDied(EntityManager source, Entity entity)
        {
            if (entity is Unit)
            {
                Unit unit = (Unit)entity ;
                if (selectedUnits.Contains(unit))
                {
                    selectedUnits.Remove(unit);
                    UpdateSelection();
                }
                if (hoveredUnit == unit) hoveredUnit = null;
            }
        }

        private void UpdateSelection()
        {
            if (selectedUnits.Count > MaxSelectedUnits)
                selectedUnits.RemoveRange(MaxSelectedUnits, selectedUnits.Count - MaxSelectedUnits);
            selectedUnits.Sort((a, b) => a.Type.Handle.Value.CompareTo(b.Type.Handle.Value));
            RaiseSelectionChanged();
        }
        #endregion
    }
}
