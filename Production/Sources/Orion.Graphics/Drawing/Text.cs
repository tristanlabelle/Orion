using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rectangle = Orion.Geometry.Rectangle;

using OpenTK.Graphics;

namespace Orion.Graphics
{
    public struct Text
    {
        #region Fields
        internal static TextPrinter defaultTextPrinter = new TextPrinter();
        public static Font DefaultFont = new Font("Consolas", 14);

        public readonly string Value;
        public readonly Font Font;
        #endregion

        #region Constructors
        public Text(string text) : this(text, DefaultFont) { }
        public Text(string text, Font font)
        {
            Value = text;
            Font = font;
        }
        #endregion

        #region Methods
        public Rectangle Frame
        {
            get
            {
                RectangleF extents = defaultTextPrinter.Measure(Value, Font).BoundingBox;
                return new Rectangle(extents.X, 0, extents.Width, extents.Height);
            }
        }
        #endregion
    }
}
