using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    public sealed class TexturedFrameRenderer : FrameRenderer
    {
        #region Fields
        public readonly Texture Texture;
        private readonly Color tint;
        #endregion

        #region Constructors
        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = Color.White;
        }

        public TexturedFrameRenderer(Texture texture, Color tint, Color strokeColor)
            : base(strokeColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = tint;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            context.Fill(context.CoordinateSystem, Texture, tint);
            base.Draw(context);
        }
        #endregion
    }
}
