using System;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public sealed class TexturedFrameRenderer : IViewRenderer
    {
        #region Fields
        private readonly Texture texture;
        private readonly ColorRgba tint = Colors.White;
        private readonly ColorRgba borderColor = Colors.Gray;
        private readonly ColorRgba backgroundColor = Colors.TransparentBlack;
        #endregion

        #region Constructors
        public TexturedFrameRenderer(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = Colors.White;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba borderColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = tint;
            this.borderColor = borderColor;
        }

        public TexturedFrameRenderer(Texture texture, ColorRgba tint, ColorRgba borderColor, ColorRgba backgroundColor)
        {
            Argument.EnsureNotNull(texture, "texture");
            this.texture = texture;
            this.tint = tint;
            this.borderColor = borderColor;
            this.backgroundColor = backgroundColor;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Fill(bounds, backgroundColor);
            context.Fill(bounds, texture, tint);
            context.Stroke(bounds, borderColor);
        }
        #endregion
    }
}
