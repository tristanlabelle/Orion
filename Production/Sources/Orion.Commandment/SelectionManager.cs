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
        private readonly List<Unit> selectedUnits = new List<Unit>();
        private readonly Faction faction;
        private Vector2 cursorPosition;
        private Vector2? selectionStartPosition;
        private bool shiftKeyPressed;
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

        /// <summary>
        /// Gets or sets if the control key is pressed or not;
        /// </summary>
        public bool ShiftKeyPressed
        {
            get { return shiftKeyPressed; }
            set { shiftKeyPressed = value; }
        }

        /// <summary>
        /// Gets the selection <see cref="Rectangle"/> being traced,
        /// or <c>null</c> if no selection <see cref="Rectangle"/> is being traced.
        /// </summary>
        public Rectangle? SelectionRectangle
        {
            get
            {
                if (!selectionStartPosition.HasValue) return null;
                return Rectangle.FromPoints(selectionStartPosition.Value, cursorPosition);
            }
        }

        /// <summary>
        /// Gets a value indicating if a selection is currently being made.
        /// </summary>
        public bool IsSelecting
        {
            get { return selectionStartPosition.HasValue; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that a <see cref="MouseButton"/> was pressed or released.
        /// </summary>
        /// <param name="clickPosition">Tells where the click happened, in world coordinates</param>
        /// <param name="button">The <see cref="MouseButton"/> that was pressed or released.</param>
        /// <param name="pressed"><c>True</c> if the button was pressed, <c>false</c> if it was released.</param>
        public void OnMouseButton(Vector2 clickPosition, MouseButton button, bool pressed)
        {
            if (button == MouseButton.Left)
            {
                if (pressed) selectionStartPosition = cursorPosition;
                else if (SelectionRectangle.HasValue)
                {
                    SelectUnits(HandleRectangleSelection());
                    selectionStartPosition = null;
                }
            }
        }

        public void SelectUnit(Unit unit)
        {
            if(!shiftKeyPressed) selectedUnits.Clear();
			if(!selectedUnits.Contains(unit))
			{
	            selectedUnits.Add(unit);
	            OnSelectionChange();
			}
        }

        public void SelectUnits(IEnumerable<Unit> units)
        {
            if (!shiftKeyPressed) selectedUnits.Clear();
            selectedUnits.AddRange(units.Except(selectedUnits));
            OnSelectionChange();
        }

        private IEnumerable<Unit> HandleRectangleSelection()
        {
            Rectangle selectionRectangle = SelectionRectangle.Value;
            List<Unit> unitsInSelectionRectangle = faction.World.Units
                .Where(unit => Intersection.Test(selectionRectangle, unit.Circle))
                .ToList();

            // Filter out buildings
            bool containsNonBuildingUnits = unitsInSelectionRectangle.Any(unit => !unit.Type.IsBuilding);
            if (containsNonBuildingUnits)
                unitsInSelectionRectangle.RemoveAll(unit => unit.Type.IsBuilding);

            // Filter out factions
            bool containsUnitsFromThisFaction = unitsInSelectionRectangle.Any(unit => unit.Faction == faction);
            if (containsUnitsFromThisFaction)
                unitsInSelectionRectangle.RemoveAll(unit => unit.Faction != faction);
            else if(unitsInSelectionRectangle.Count > 1)
                unitsInSelectionRectangle.RemoveRange(1, unitsInSelectionRectangle.Count - 1);

            return unitsInSelectionRectangle;
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that the cursor moved.
        /// </summary>
        /// <param name="location">The new location of the cursor, in world units.</param>
        public void OnMouseMove(Vector2 location)
        {
            cursorPosition = location;
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that a unit died.
        /// </summary>
        /// <param name="source">The source UnitRegistry</param>
        /// <param name="unit">The killed unit</param>
        public void UnitDied(UnitRegistry source, Unit unit)
        {
            if (selectedUnits.Contains(unit))
            {
                selectedUnits.Remove(unit);
                OnSelectionChange();
            }
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that they control key state has changed.
        /// </summary>
        /// <param name="shiftKeyState">The source UnitRegistry</param>
        public void OnShiftKeyChanged(bool shiftKeyState)
        {
            ShiftKeyPressed = shiftKeyState;
        }

        private void OnSelectionChange()
        {
            selectedUnits.Sort((a, b) => a.Type.ID.CompareTo(b.Type.ID));
            GenericEventHandler<SelectionManager> handler = SelectionChanged;
            if (handler != null) handler(this);
        }

        #endregion
    }
}
