using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2.Adornments
{
    /// <summary>
    /// A control adornment which tiles a texture under the control.
    /// </summary>
    public sealed class TilingTextureAdornment : IAdornment
    {
        #region Fields
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TilingTextureAdornment(Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");

            this.texture = texture;
        }
        #endregion

        #region Properties
        public Texture Texture
        {
            get { return texture; }
        }

        public Borders SuggestedPadding
        {
            get { return new Borders(texture.Width / 2 - 1, texture.Height / 2 - 1); }
        }
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            var sprite = new GuiSprite
            {
                Rectangle = control.Rectangle,
                Texture = texture,
                PixelRectangle = control.Rectangle
            };
            renderer.DrawSprite(ref sprite);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
