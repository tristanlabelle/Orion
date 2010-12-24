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
    /// A <see cref="UIElement"/> which displays text.
    /// </summary>
    public sealed class Label : UIElement
    {
        #region Fields
        private string text;
        private ColorRgba? color;
        private Font font;
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

        #region Events
        /// <summary>
        /// Raised when the text of this <see cref="Label"/> changes.
        /// </summary>
        public event Action<Label> TextChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the text displayed in this <see cref="Label"/>.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                Argument.EnsureNotNull(value, "Text");
                if (object.ReferenceEquals(value, text)) return;

                this.text = value;
                TextChanged.Raise(this);
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the color of this <see cref="Label"/>.
        /// A value of <c>null</c> indicates that the default color should be used.
        /// </summary>
        public ColorRgba? Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Accesses the font of this <see cref="Label"/>.
        /// A value of <c>null</c> indicates that the default font should be used.
        /// </summary>
        public Font Font
        {
            get { return font; }
            set { font = value; }
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
