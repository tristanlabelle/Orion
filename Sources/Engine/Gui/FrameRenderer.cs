using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public abstract class FrameRenderer : IViewRenderer
    {
        #region Fields
        public readonly ColorRgba StrokeColor;
        #endregion

        #region Contructors
        public FrameRenderer()
        {
            StrokeColor = Colors.Gray;
        }

        public FrameRenderer(ColorRgba strokeColor)
        {
            StrokeColor = strokeColor;
        }
        #endregion

        #region Methods
        public virtual void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Stroke(bounds, StrokeColor);
        }
        #endregion
    }
}