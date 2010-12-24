using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A UIElement which displays text.
    /// </summary>
    public sealed class Label : UIElement
    {
        #region Fields
        private string text;
        private ColorRgba? customColor;
        private Font customFont;
        #endregion

        #region Constructors
        public Label()
        {
            text = string.Empty;
        }

        public Label(string text)
        {
            Argument.EnsureNotNull(text, "text");
            this.text = text;
        }
        #endregion

        #region Properties
        public string Text
        {
            get { return text; }
            set
            {
                Argument.EnsureNotNull(value, "Text");
                if (object.ReferenceEquals(value, text)) return;

                this.text = value;
                InvalidateMeasure();
            }
        }

        public ColorRgba? CustomColor
        {
            get { return customColor; }
            set { customColor = value; }
        }
        
        public ColorRgba Color
        {
            get
            {
                if (customColor.HasValue) return customColor.Value;
                return Manager == null ? Colors.Black : Manager.DefaultTextColor;
            }
        }
        
        public Font CustomFont
        {
            get { return customFont; }
            set { customFont = value; }
        }
        
        public Font Font
        {
            get
            {
                if (customFont != null) return customFont;
                return Manager == null ? null : Manager.DefaultFont;
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureWithoutMargin()
        {
            return Manager.Renderer.MeasureText(this, text);
        }
        #endregion
    }
}
