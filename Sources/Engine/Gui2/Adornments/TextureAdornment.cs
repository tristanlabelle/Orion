﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2.Adornments
{
    public sealed class TextureAdornment : IAdornment
    {
        #region Fields
        private readonly Texture texture;
        #endregion

        #region Constructors
        public TextureAdornment(Texture texture)
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
        #endregion

        #region Methods
        public void DrawBackground(GuiRenderer renderer, Control control)
        {
            var sprite = new GuiSprite(control.Rectangle, texture);
            renderer.DrawSprite(ref sprite);
        }

        public void DrawForeground(GuiRenderer renderer, Control control) { }
        #endregion
    }
}
