using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A <see cref="Control"/> which displays text.
    /// </summary>
    public class Label : Control
    {
        #region Fields
        private string text;
        private ColorRgba textColor = Colors.Black;
        private ColorRgba disabledTextColor = Colors.Gray;
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
        /// Accesses the color of the text of this <see cref="Label"/>.
        /// </summary>
        public ColorRgba TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        /// <summary>
        /// Accesses the color of the text when this <see cref="TextField"/> is disabled.
        /// </summary>
        public ColorRgba DisabledTextColor
        {
            get { return disabledTextColor; }
            set { disabledTextColor = value; }
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
        protected override Size MeasureSize(Size availableSize)
        {
            var options = new TextRenderingOptions
            {
                Font = font
            };

            return Renderer.MeasureText(text, ref options);
        }

        protected override void ArrangeChildren() { }

        protected internal override void Draw()
        {
            var options = new TextRenderingOptions
            {
                Origin = Rectangle.Min,
                Font = font,
                Color = IsEnabled ? textColor : disabledTextColor
            };

            Renderer.DrawText(text, ref options);
        }
        #endregion
    }
}
