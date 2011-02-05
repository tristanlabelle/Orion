using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui.Adornments
{
    /// <summary>
    /// A control adornment which draws a border based on a texture.
    /// </summary>
    public sealed class BorderTextureAdornment : IAdornment
    {
        #region Fields
        private readonly Texture texture;
        #endregion

        #region Constructors
        public BorderTextureAdornment(Texture texture)
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
            renderer.DrawNinePart(control.Rectangle, texture, Colors.White);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
