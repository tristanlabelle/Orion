using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which displays an image.
    /// </summary>
    public sealed class ImageBox : Control
    {
        #region Fields
        private Texture texture;
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
        #endregion

        #region Methods
        protected override Size MeasureSize()
        {
            return texture == null ? Size.Zero : texture.Size;
        }

        protected internal override void Draw()
        {
            Region rectangle;
            if (!TryGetRectangle(out rectangle)) return;

            Renderer.Fill(rectangle, texture, Colors.White);
        }
        #endregion
    }
}
