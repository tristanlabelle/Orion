using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which displays an image.
    /// </summary>
    public sealed class ImageBox : Control
    {
        #region Fields
        private Texture texture;
        private Stretch stretch = Stretch.Uniform;
        private ColorRgba color = Colors.White;
        private bool drawIfNoTexture;
        #endregion

        #region Constructors
        public ImageBox() { }

        public ImageBox(Texture texture)
        {
            this.texture = texture;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Texture"/> used by this <see cref="ImageBox"/>.
        /// </summary>
        public Texture Texture
        {
            get { return texture; }
            set
            {
                if (value == texture) return;

                texture = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the way the image is stretched to fill the available space.
        /// </summary>
        public Stretch Stretch
        {
            get { return stretch; }
            set { stretch = value; }
        }

        /// <summary>
        /// Accesses the color modulation of the drawn image.
        /// </summary>
        public ColorRgba Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Accesses the tint of the drawn image.
        /// </summary>
        public ColorRgb Tint
        {
            get { return color.Rgb; }
            set { color = new ColorRgba(value, color.A); }
        }

        /// <summary>
        /// Accesses a value which indicates if a colored rectangle should be drawn if <see cref="P:Texture"/> is null.
        /// </summary>
        public bool DrawIfNoTexture
        {
            get { return drawIfNoTexture; }
            set { drawIfNoTexture = value; }
        }
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize)
        {
            return texture == null ? Size.Zero : new Size(texture.Width, texture.Height);
        }

        protected override void ArrangeChildren() { }

        protected internal override void Draw()
        {
            if (texture == null)
            {
                if (drawIfNoTexture) Renderer.DrawRectangle(Rectangle, Color);
                return;
            }

            var rectangle = Rectangle;
            var sprite = new GuiSprite
            {
                Texture = texture,
                PixelRectangle = new Region(texture.Width, texture.Height),
                Color = color
            };

            switch (stretch)
            {
                case Stretch.Fill:
                    sprite.Rectangle = rectangle;
                    break;

                case Stretch.None:
                    sprite.Rectangle = new Region(
                        (int)Math.Round(rectangle.MinX + rectangle.Width / 2f - texture.Width / 2f),
                        (int)Math.Round(rectangle.MinY + rectangle.Height / 2f - texture.Height / 2f),
                        texture.Width, texture.Height);
                    break;

                case Stretch.Uniform:
                    float rectangleAspectRatio = rectangle.Height / (float)rectangle.Width;
                    float imageAspectRatio = texture.Height / (float)texture.Width;
                    if (rectangleAspectRatio > imageAspectRatio)
                    {
                        int height = (int)Math.Round(rectangle.Width * imageAspectRatio);
                        sprite.Rectangle = new Region(
                            rectangle.MinX,
                            (int)Math.Round(rectangle.MinY + rectangle.Height / 2f - height / 2f),
                            rectangle.Width, height);
                    }
                    else
                    {
                        int width = (int)Math.Round(rectangle.Height / imageAspectRatio);
                        sprite.Rectangle = new Region(
                            (int)Math.Round(rectangle.MinX + rectangle.Width / 2f - width / 2f),
                            rectangle.MinY,
                            width, rectangle.Height);
                    }
                    break;

                default:
                    Debug.Fail("Unexpected image stretch: " + stretch);
                    return;
            }

            Renderer.DrawSprite(ref sprite);
        }
        #endregion
    }
}
