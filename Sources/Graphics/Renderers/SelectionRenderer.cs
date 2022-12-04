using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Draws selection-related information on-screen.
    /// </summary>
    public sealed class SelectionRenderer
    {
        #region Fields
        private static readonly ColorRgba selectionMarkerColor = ColorRgba.FromBytes(51, 153, 255);
        private static readonly ColorRgba selectionRectangleStrokeColor = ColorRgba.FromBytes(51, 153, 255);
        private static readonly ColorRgba selectionRectangleFillColor = ColorRgba.FromBytes(51, 153, 255, 100);

        private readonly UICommander uiCommander;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SelectionRenderer"/> from a
        /// <see cref="SelectionManager"/> providing information
        /// on the current selection.
        /// </summary>
        /// <param name="selectionManager">
        /// The <see cref="SelectionManager"/> which provides selection information.
        /// </param>
        public SelectionRenderer(UICommander manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            uiCommander = manager;
        }
        #endregion

        #region Properties
        private SelectionManager SelectionManager
        {
            get { return uiCommander.SelectionManager; }
        }

        private Faction Faction
        {
            get { return uiCommander.Faction; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the selection markers under the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawSelectionMarkers(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            foreach (Unit unit in SelectionManager.SelectedUnits)
            {
                if (!SelectionManager.Faction.CanSee(unit))
                    continue;

                graphics.Stroke(unit.BoundingRectangle, selectionMarkerColor);

                if (unit.Faction == Faction && unit.HasRallyPoint)
                {
                    LineSegment lineSegment = new LineSegment(unit.Center, unit.RallyPoint.Value);
                    graphics.Stroke(lineSegment, selectionMarkerColor);
                }
            }
        }

        /// <summary>
        /// Draws the selection markers over the <see cref="Unit"/>s.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsContext"/> to be used for rendering.</param>
        public void DrawSelectionRectangle(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            
            if (uiCommander.SelectionRectangle.HasValue)
            {
                Rectangle selectionRectangle = uiCommander.SelectionRectangle.Value;
                graphics.Stroke(selectionRectangle, selectionRectangleStrokeColor);
                graphics.Fill(selectionRectangle, selectionRectangleFillColor);
            }
        }
        #endregion
    }
}
