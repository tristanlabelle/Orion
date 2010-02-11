using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public abstract class FrameRenderer : IRenderer
    {
        #region Fields
        public readonly Color StrokeColor;
        #endregion

        #region Contructors
        public FrameRenderer()
        {
            StrokeColor = Color.Gray;
        }

        public FrameRenderer(Color strokeColor)
        {
            StrokeColor = strokeColor;
        }
        #endregion

        #region Methods
        public virtual void Draw(GraphicsContext context)
        {
            context.StrokeColor = StrokeColor;
            context.Stroke(context.CoordinateSystem);
        }
        #endregion
    }
}