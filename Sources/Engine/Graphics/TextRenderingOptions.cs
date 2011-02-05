using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Orion.Engine.Graphics
{
    public struct TextRenderingOptions
    {
        #region Fields
        private Point origin;
        private Font font;
        private int? maxWidthInPixels;
        private TextOverflowPolicy horizontalOverflowPolicy;
        private int? maxHeightInPixels;
        private ColorRgb color;
        private float transparency;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the location from where the text should start to be drawn.
        /// </summary>
        public Point Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        /// <summary>
        /// Accesses the font to be used.
        /// </summary>
        public Font Font
        {
            get { return font ?? SystemFonts.DefaultFont; }
            set { font = value ?? SystemFonts.DefaultFont; }
        }

        /// <summary>
        /// Accesses an optional max width to the area of rendered text.
        /// If the width of the text overflows this, the policy specified by <see cref="HorizontalOverflowPolicy"/> is applied.
        /// </summary>
        public int? MaxWidthInPixels
        {
            get { return maxWidthInPixels; }
            set
            {
                if (value.HasValue) Argument.EnsurePositive(value.Value, "MaxWidthInPixels");
                maxWidthInPixels = value;
            }
        }

        /// <summary>
        /// Accesses a value specifying how to handle cases where the text is wider than the maximum width.
        /// </summary>
        public TextOverflowPolicy HorizontalOverflowPolicy
        {
            get { return horizontalOverflowPolicy; }
            set
            {
                Argument.EnsureDefined(value, "HorizontalOverflowPolicy");
                horizontalOverflowPolicy = value;
            }
        }

        public int? MaxHeightInPixels
        {
            get { return maxHeightInPixels; }
            set
            {
                if (value.HasValue) Argument.EnsurePositive(value.Value, "MaxHeightInPixels");
                maxHeightInPixels = value;
            }
        }

        /// <summary>
        /// Accesses the color of the text to be rendered.
        /// </summary>
        public ColorRgba Color
        {
            get { return color.ToRgba(1 - transparency); }
            set
            {
                value = ColorRgba.Clamp(value);
                color = value.Rgb;
                transparency = 1 - value.A;
            }
        }
        #endregion

        #region Methods
        #endregion
    }
}
