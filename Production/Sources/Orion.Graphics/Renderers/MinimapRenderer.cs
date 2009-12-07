using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class MinimapRenderer : FrameRenderer
    {
        #region Fields
        private readonly WorldRenderer worldRenderer;
        #endregion

        #region Constructors
        public MinimapRenderer(WorldRenderer worldRenderer)
        {
            Argument.EnsureNotNull(worldRenderer, "worldRenderer");
            this.worldRenderer = worldRenderer;
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
            worldRenderer.UnitRenderer.DrawMiniature(context);
            worldRenderer.DrawFogOfWar(context);

            context.StrokeColor = Color.Orange;
            Rectangle? intersection = Rectangle.Intersection(context.CoordinateSystem, VisibleRect);
            context.Stroke(intersection.GetValueOrDefault());
            context.StrokeColor = Color.Gray;
            context.Stroke(context.CoordinateSystem);
        }
        #endregion
    }
}
