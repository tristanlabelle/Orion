﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.Graphics
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

        #region Properties
        /// <summary>
        /// Gets the sequence of units currently selected.
        /// </summary>
        public IEnumerable<Unit> SelectedUnits
        {
            get
            {
                if (selectedUnits.Count == 1 || selectedUnits.Count == 0)
                    return selectedUnits;
                else if (selectedUnits.Where(unit => unit.Faction == faction).ToList().Count > 0)
                    return selectedUnits.Where(unit => unit.Faction == faction).ToList();
                else
                {
                    List<Unit> firstEnemyUnit = new List<Unit>();
                    firstEnemyUnit.Add(selectedUnits[0]);
                    return firstEnemyUnit;
                }    

            }
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
        /// <param name="button">The <see cref="MouseButton"/> that was pressed or released.</param>
        /// <param name="pressed"><c>True</c> if the button was pressed, <c>false</c> if it was released.</param>
        public void OnMouseButton(Vector2 clickPosition, MouseButton button, bool pressed)
        {
            if (button == MouseButton.Left)
            {
                if (pressed)
                {
                    selectionStartPosition = cursorPosition;
                }
                else if(SelectionRectangle.HasValue)
                {
                    Rectangle selectionRectangle = SelectionRectangle.Value;

                    var units = faction.World.Units.Where(unit => Intersection.RectangleIntersectsCircle(selectionRectangle, unit.Circle));

                    selectedUnits.Clear();
                    selectedUnits.AddRange(units);

                    selectionStartPosition = null;
                }
            }
        }

        /// <summary>
        /// Informs this <see cref="SelectionManager"/> that the cursor moved.
        /// </summary>
        /// <param name="location">The new location of the cursor, in world units.</param>
        public void OnMouseMove(Vector2 location)
        {
            cursorPosition = location;
        }
        #endregion
    }
}
