using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rectangle = Orion.Geometry.Rectangle;
using OpenTK.Math;
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
        public Text(string text)
            : this(text, DefaultFont)
        { }

        public Text(string text, Font font)
        {
            Value = text;
            Font = font;
        }
        #endregion

        #region Properties
        public Rectangle Frame
        {
            get
            {
                RectangleF extents = defaultTextPrinter.Measure(Value, Font).BoundingBox;
                return new Rectangle(extents.X, 0, extents.Width, extents.Height);
            }
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
            RectangleF extents = defaultTextPrinter.Measure(Value, Font, constraint).BoundingBox;
            return new Rectangle(extents.X, 0, extents.Width, extents.Height);
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
