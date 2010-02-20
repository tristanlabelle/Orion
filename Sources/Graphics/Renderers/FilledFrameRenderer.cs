using System;

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
        public override void Draw(GraphicsContext context)
        {
            context.FillColor = FillColor;
            context.Fill(context.CoordinateSystem);
            base.Draw(context);
        }
        #endregion
    }
}
