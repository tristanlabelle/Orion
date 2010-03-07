using System;
using Orion.Engine.Graphics;
using Orion.Geometry;

namespace Orion.Graphics
{
    public sealed class FilledFrameRenderer : FrameRenderer
    {
        #region Fields
        public readonly ColorRgba FillColor;
        #endregion

        #region Constructors
        public FilledFrameRenderer()
        {
            FillColor = Colors.DarkGray;
        }

        public FilledFrameRenderer(ColorRgba fillColor)
        {
            FillColor = fillColor;
        }

        public FilledFrameRenderer(ColorRgba fillColor, ColorRgba strokeColor)
            : base(strokeColor)
        {
            FillColor = fillColor;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Fill(bounds, FillColor);
            base.Draw(context, bounds);
        }
        #endregion
    }
}
