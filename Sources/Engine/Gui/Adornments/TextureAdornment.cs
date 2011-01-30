using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui.Adornments
{
    /// <summary>
    /// An adornment which draws a texture in the background of a control.
    /// </summary>
    public sealed class TextureAdornment : IAdornment
    {
        #region Fields
        private Texture texture;
        private bool isTiling;
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

        /// <summary>
        /// Accesses a value indicating if the texture should tile accross the surface of the control.
        /// </summary>
        public bool IsTiling
        {
            get { return isTiling; }
            set { isTiling = value; }
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            if (texture == null) return;

            var sprite = new GuiSprite(control.Rectangle, texture);
            if (isTiling) sprite.PixelRectangle = (Region)control.ActualSize;
            renderer.DrawSprite(ref sprite);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
