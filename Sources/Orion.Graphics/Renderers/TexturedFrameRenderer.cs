using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class TexturedFrameRenderer : FrameRenderer
    {
        #region Fields
        public readonly Texture Texture;
        private readonly Color tint;
        private readonly Color background;
        #endregion

        #region Constructors
        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = Color.White;
            this.background = Color.Transparent;
        }

        public TexturedFrameRenderer(Texture texture, Color tint, Color strokeColor)
            : base(strokeColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = tint;
            this.background = Color.Transparent;
        }

        public TexturedFrameRenderer(Texture texture, Color tint, Color strokeColor, Color backgroundColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = tint;
            this.background = backgroundColor;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            context.FillColor = background;
            context.Fill(context.CoordinateSystem);
            context.Fill(context.CoordinateSystem, Texture, tint);
            base.Draw(context);
        }
        #endregion
    }
}
