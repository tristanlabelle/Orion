using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public sealed class Label : UIElement
    {
        #region Fields
        private string text = string.Empty;
        private ColorRgba color = Colors.White;
        private Size? size;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public string Text
        {
            get { return text; }
            set
            {
                Argument.EnsureNotNull(value, "Text");
                this.text = value;

            }
        }

        public ColorRgba Color
        {
            get { return color; }
            set { color = value; }
        }
        #endregion

        #region Methods
        protected override Size Measure(Size? constrainedSize)
        {
            
        }
        #endregion
    }
}
