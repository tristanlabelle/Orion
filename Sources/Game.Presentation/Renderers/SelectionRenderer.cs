using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;

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
        /// Draws the selection markers under the <see cref="Entity"/>.
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
                FactionMembership membership = entity.Components.TryGet<FactionMembership>();
                if (membership != null && membership.Faction == Faction)
                {
                    Trainer train = entity.Components.TryGet<Trainer>();
                    if (train != null)
                    {
                        Spatial spatial = entity.Spatial;
                        LineSegment lineSegment = new LineSegment(spatial.Center, train.RallyPoint);
                        graphics.Stroke(lineSegment, selectionMarkerColor);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the selection markers over the <see cref="Entity"/>s.
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
