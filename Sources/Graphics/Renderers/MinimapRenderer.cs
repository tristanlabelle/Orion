using System;
using System.Diagnostics;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    public sealed class MinimapRenderer : FrameRenderer
    {
        #region Fields
        private readonly WorldRenderer worldRenderer;
        private readonly AttackWarningRenderer attackWarningRenderer;
        #endregion

        #region Constructors
        public MinimapRenderer(WorldRenderer worldRenderer)
        {
            Argument.EnsureNotNull(worldRenderer, "worldRenderer");

            this.worldRenderer = worldRenderer;
            this.attackWarningRenderer = new AttackWarningRenderer(worldRenderer.Faction);
        }
        #endregion

        #region Properties
        internal Rectangle VisibleRect { get; set; }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            worldRenderer.DrawMiniatureTerrain(context, bounds);
            worldRenderer.DrawMiniatureResources(context, bounds);
            worldRenderer.DrawMiniatureUnits(context, bounds);
            worldRenderer.DrawFogOfWar(context, bounds);
            attackWarningRenderer.Draw(context);

            context.StrokeColor = Colors.Orange;
            Rectangle? intersection = Rectangle.Intersection(bounds, VisibleRect);
            context.Stroke(intersection.GetValueOrDefault());
            context.StrokeColor = Colors.Gray;
            context.Stroke(bounds);
        }
        #endregion
    }
}
