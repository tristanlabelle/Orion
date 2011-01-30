using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Parameter class which wraps all necessary data to draw a sprite.
    /// </summary>
    public struct GuiSprite
    {
        #region Fields
        public Region Rectangle;
        public Texture Texture;
        public Region PixelRectangle;
        private ColorRgba oneMinusColor;
        #endregion

        #region Constructors
        public GuiSprite(Texture texture)
        {
            this.Rectangle = default(Region);
            this.Texture = texture;
            this.PixelRectangle = texture == null ? default(Region) : new Region(texture.Size);
            this.oneMinusColor = default(ColorRgba);
        }

        public GuiSprite(Region rectangle, Texture texture)
        {
            this.Rectangle = rectangle;
            this.Texture = texture;
            this.PixelRectangle = texture == null ? default(Region) : new Region(texture.Size);
            this.oneMinusColor = default(ColorRgba);
        }
        #endregion

        #region Properties
        public ColorRgba Color
        {
            get { return new ColorRgba(1 - oneMinusColor.R, 1 - oneMinusColor.G, 1 - oneMinusColor.B, 1 - oneMinusColor.A); }
            set { oneMinusColor = new ColorRgba(1 - value.R, 1 - value.G, 1 - value.B, 1 - value.A); }
        }

        public ColorRgb Tint
        {
            get { return new ColorRgb(1 - oneMinusColor.R, 1 - oneMinusColor.G, 1 - oneMinusColor.B); }
            set { oneMinusColor = new ColorRgba(1 - value.R, 1 - value.G, 1 - value.B, oneMinusColor.A); }
        }

        public float Alpha
        {
            get { return 1 - oneMinusColor.A; }
            set { oneMinusColor = new ColorRgba(Tint, 1 - value); }
        }
        #endregion

        #region Methods
        #endregion
    }
}
