using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;
using Orion.Engine.Input;
using Key = OpenTK.Input.Key;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which presents an editable box of text to the user.
    /// </summary>
    public sealed class TextField : Control
    {
        #region Fields
        private string text = string.Empty;
        private int caretIndex;
        private bool isEditable = true;

        private Font font;
        private int cachedFontHeight;

        private ColorRgba textColor = Colors.Black;
        private ColorRgba caretColor = Colors.Black;
        private int caretWidth = System.Windows.Forms.SystemInformation.CaretWidth;
        private Borders padding;
        #endregion

        #region Constructors
        public TextField(string text)
        {
            Argument.EnsureNotNull(text, "text");

            this.text = text;
            caretIndex = text.Length;
            MinSize = new Size(60, 20);
        }

        public TextField()
            : this(string.Empty) { }
        #endregion

        #region Event
        /// <summary>
        /// Raised when the text within this <see cref="TextField"/> changes.
        /// </summary>
        public event Action<TextField> TextChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the text that within this text field.
        /// </summary>
        public string Text
        {
            get { return text.ToString(); }
            set
            {
                Argument.EnsureNotNull(text, "text");
                if (object.ReferenceEquals(value, text)) return;

                text = value;
                caretIndex = text.Length;

                TextChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the current index of the caret.
        /// </summary>
        public int CaretIndex
        {
            get { return caretIndex; }
            set
            {
                if (value == caretIndex) return;
                if (value < 0 || value > text.Length) throw new ArgumentOutOfRangeException("Caret index out of text bounds.");

                caretIndex = value;
            }
        }

        /// <summary>
        /// Accesses a value which determines if the user can edit the text in this <see cref="TextField"/>.
        /// </summary>
        public bool IsEditable
        {
            get { return isEditable; }
            set
            {
                if (value == isEditable) return;

                isEditable = value;
                if (!isEditable && HasKeyboardFocus) Manager.KeyboardFocusedControl = null;
            }
        }

        /// <summary>
        /// Accesses the font of the text in this <see cref="TextField"/>.
        /// </summary>
        public Font Font
        {
            get { return font; }
            set
            {
                if (value == font) return;

                font = value;
                cachedFontHeight = font == null ? 0 : font.Height;

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the color of the (unselected) text in this <see cref="TextField"/>.
        /// </summary>
        public ColorRgba TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        /// <summary>
        /// Accesses the padding between the borders of the <see cref="TextField"/>.
        /// </summary>
        public Borders Padding
        {
            get { return padding; }
            set
            {
                if (value == padding) return;

                padding = value;
                InvalidateMeasure();
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize)
        {
            return new Size(padding.TotalX, padding.TotalY + cachedFontHeight);
        }

        protected internal override void Draw()
        {
            Region rectangle = Rectangle;
            if (rectangle.Width < padding.TotalX || rectangle.Height < padding.TotalY) return;

            var options = new TextRenderingOptions
            {
                Font = font,
                Color = textColor,
                Origin = new Point(rectangle.MinX + padding.MinX, rectangle.MinY + padding.MinY)
            };
            Renderer.DrawText(text, ref options);
        }

        protected override void ArrangeChildren() { }

        protected override bool OnMouseButton(MouseEvent @event)
        {
            if (@event.Button == MouseButtons.Left && @event.IsPressed && isEditable)
            {
                AcquireKeyboardFocus();
                return true;
            }

            return false;
        }

        protected override bool OnMouseClick(MouseEvent @event)
        {
            if (IsEditable) AcquireKeyboardFocus();

            return true;
        }

        protected override bool OnKeyEvent(KeyEvent @event)
        {
            if (!@event.IsDown || @event.ModifierKeys != ModifierKeys.None) return true;

            switch (@event.Key)
            {
                case Key.Left:
                    if (caretIndex > 0) --caretIndex;
                    break;

                case Key.Right:
                    if (caretIndex < text.Length) ++caretIndex;
                    break;

                case Key.Home:
                    caretIndex = 0;
                    break;

                case Key.End:
                    caretIndex = text.Length;
                    break;

                case Key.Back:
                    if (caretIndex > 0)
                    {
                        Text = text.Substring(0, caretIndex - 1) + text.Substring(caretIndex);
                        --caretIndex;
                    }
                    break;

                case Key.Delete:
                    if (caretIndex < text.Length)
                    {
                        Text = text.Substring(0, caretIndex) + text.Substring(caretIndex + 1);
                    }
                    break;
            }

            return true;
        }

        protected override bool OnCharacterTyped(char character)
        {
            if (!"\b\r\t\n".Contains(character))
            {
                Text = text.Insert(caretIndex, character);
                ++caretIndex;
            }

            return true;
        }
        #endregion
    }
}
