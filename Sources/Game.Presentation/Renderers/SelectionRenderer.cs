using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Engine.Geometry;

namespace Orion.Game.Presentation.Renderers
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

        private readonly UserInputManager userInputManager;
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
        public SelectionRenderer(UserInputManager manager)
        {
            Argument.EnsureNotNull(manager, "manager");
            userInputManager = manager;
        }
        #endregion

        #region Properties
        private Selection Selection
        {
            get { return userInputManager.Selection; }
        }

        private Faction Faction
        {
            get { return userInputManager.LocalFaction; }
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

            foreach (Entity entity in Selection)
            {
                if (!Faction.CanSee(entity)) continue;
                graphics.Stroke(entity.BoundingRectangle, selectionMarkerColor);
            }
        }

        public void DrawRallyPointMarkers(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            foreach (Entity entity in Selection)
            {
                Unit unit = entity as Unit;
                if (unit != null && unit.Faction == Faction && unit.HasRallyPoint)
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
            
            if (userInputManager.SelectionRectangle.HasValue)
            {
                Rectangle selectionRectangle = userInputManager.SelectionRectangle.Value;
                graphics.Stroke(selectionRectangle, selectionRectangleStrokeColor);
                graphics.Fill(selectionRectangle, selectionRectangleFillColor);
            }
        }
        #endregion
    }
}
