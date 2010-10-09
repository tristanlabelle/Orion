using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using Rectangle = Orion.Engine.Geometry.Rectangle;

namespace Orion.Engine.Graphics
{
    public struct Text
    {
        #region Fields
        public static Font DefaultFont = new Font("Trebuchet MS", 14);

        private readonly string value;
        public readonly Font Font;
        #endregion

        #region Constructors
        public Text(string text)
            : this(text, DefaultFont)
        { }

        public Text(string text, Font font)
        {
            value = text + " ";
            Font = font;
        }
        #endregion

        #region Properties
        public string Value
        {
            get { return value.Substring(0, value.Length - 1); }
        }

        public Rectangle Frame
        {
            get { return new Rectangle(value.Length * Font.Height * 0.4f, Font.Height); }
        }
        #endregion

        #region Methods
        public float HeightForConstrainedWidth(float width)
        {
            RectangleF constraint = new RectangleF(0, 0, width, float.MaxValue);
            return MeasureConstrained(constraint).Height;
        }

        public float WidthForConstrainedHeight(float height)
        {
            RectangleF constraint = new RectangleF(0, 0, float.MaxValue, height);
            return MeasureConstrained(constraint).Width;
        }
        
        private Rectangle MeasureConstrained(RectangleF constraint)
        {
            return Frame;
        }

        #region Object Model
        public override string ToString()
        {
            return Value;
        }
        #endregion
        #endregion
    }
}
