using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class FilledFrameRenderer : FrameRenderer
    {
        #region Fields
        public readonly Color FillColor;
        #endregion

        #region Constructors
        public FilledFrameRenderer()
        {
            FillColor = Color.DarkGray;
        }

        public FilledFrameRenderer(Color fillColor)
        {
            FillColor = fillColor;
        }

        public FilledFrameRenderer(Color fillColor, Color strokeColor)
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
