using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Renderers
{
    public sealed class MinimapRenderer : IViewRenderer
    {
        #region Fields
        private static readonly ColorRgb ViewRectangleColor = Colors.Orange;
        private static readonly ColorRgb BorderColor = Colors.Gray;

        private readonly WorldRenderer worldRenderer;
        private readonly UnderAttackWarningRenderer attackWarningRenderer;
        #endregion

        #region Constructors
        public MinimapRenderer(WorldRenderer worldRenderer)
        {
            Argument.EnsureNotNull(worldRenderer, "worldRenderer");

            this.worldRenderer = worldRenderer;
            this.attackWarningRenderer = new UnderAttackWarningRenderer(worldRenderer.Faction);
        }
        #endregion

        #region Properties
        internal Rectangle VisibleRect { get; set; }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            worldRenderer.DrawMiniatureTerrain(context, bounds);
            worldRenderer.DrawMiniatureResources(context, bounds);
            worldRenderer.DrawMiniatureUnits(context, bounds);
            worldRenderer.DrawFogOfWar(context, bounds);
            attackWarningRenderer.Draw(context);

            Rectangle intersection = Rectangle.Intersection(bounds, VisibleRect).GetValueOrDefault();
            context.Stroke(intersection, ViewRectangleColor);
            context.Stroke(bounds, BorderColor);
        }
        #endregion
    }
}
