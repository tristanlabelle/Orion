using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2.Adornments
{
    public sealed class TextureAdornment : IAdornment
    {
        #region Fields
        private Texture texture;
        #endregion

        #region Constructors
        public TextureAdornment(Texture texture)
        {
            this.texture = texture;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the texture drawn by this adornment.
        /// </summary>
        public Texture Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            if (texture == null) return;
            var sprite = new GuiSprite(control.Rectangle, texture);
            renderer.DrawSprite(ref sprite);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
