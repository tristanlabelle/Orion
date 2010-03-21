using System;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Graphics.Renderers
{
    public sealed class TexturedFrameRenderer : FrameRenderer
    {
        #region Fields
        private readonly Texture texture;
        private readonly ColorRgba tint;
        private readonly ColorRgba backgroundColor;
        #endregion

        #region Constructors
        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = Colors.White;
            this.backgroundColor = Colors.TransparentBlack;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba strokeColor)
            : base(strokeColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = tint;
            this.backgroundColor = Colors.TransparentBlack;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba strokeColor, ColorRgba backgroundColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = tint;
            this.backgroundColor = backgroundColor;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Fill(bounds, backgroundColor);
            context.Fill(bounds, texture, tint);
            base.Draw(context, bounds);
        }
        #endregion
    }
}
