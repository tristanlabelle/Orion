using System;
using Orion.Engine.Graphics;
using Orion.Geometry;

namespace Orion.Graphics
{
    public sealed class TexturedFrameRenderer : FrameRenderer
    {
        #region Fields
        public readonly Texture Texture;
        private readonly ColorRgba tint;
        private readonly ColorRgba background;
        #endregion

        #region Constructors
        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = Colors.White;
            this.background = Colors.TransparentBlack;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba strokeColor)
            : base(strokeColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = tint;
            this.background = Colors.TransparentBlack;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba strokeColor, ColorRgba backgroundColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.Texture = texture;
            this.tint = tint;
            this.background = backgroundColor;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.FillColor = background;
            context.Fill(bounds);
            context.Fill(bounds, Texture, tint);
            base.Draw(context, bounds);
        }
        #endregion
    }
}