using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;
using System.Diagnostics;

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
        public override void Draw(GraphicsContext context)
        {
            worldRenderer.DrawMiniatureTerrain(context);
            worldRenderer.DrawMiniatureResources(context);
            worldRenderer.DrawMiniatureUnits(context);
            worldRenderer.DrawFogOfWar(context);
            attackWarningRenderer.Draw(context);

            context.StrokeColor = Colors.Orange;
            Rectangle? intersection = Rectangle.Intersection(context.CoordinateSystem, VisibleRect);
            context.Stroke(intersection.GetValueOrDefault());
            context.StrokeColor = Colors.Gray;
            context.Stroke(context.CoordinateSystem);
        }
        #endregion
    }
}
