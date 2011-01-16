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
        private string text;
        private int caretIndex;

        /// <summary>
        /// The size of the selection relative to the caret index.
        /// A negative value means that the selection is before the caret
        /// while a positive value means that the selection follows the caret.
        /// </summary>
        private int relativeSelectionLength;
        private bool isEditable = true;
        private Font font;
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
            get { return text; }
            set
            {
                Argument.EnsureNotNull(text, "text");
                if (object.ReferenceEquals(value, text)) return;

                text = value;
                if (SelectionEndIndex > text.Length)
                {
                    caretIndex = text.Length;
                    relativeSelectionLength = 0;
                }

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
        /// Gets a value indicating if part of the <see cref="TextField"/> is selected.
        /// </summary>
        public bool HasSelection
        {
            get { return relativeSelectionLength != 0; }
        }

        /// <summary>
        /// Gets the index of the beginning of the selection.
        /// </summary>
        public int SelectionStartIndex
        {
            get { return Math.Min(caretIndex, caretIndex + relativeSelectionLength); }
        }

        /// <summary>
        /// Gets the length of the selection in characters.
        /// </summary>
        public int SelectionLength
        {
            get { return Math.Abs(relativeSelectionLength); }
        }

        /// <summary>
        /// Gets the index of the end of the selection.
        /// </summary>
        public int SelectionEndIndex
        {
            get { return Math.Max(caretIndex, caretIndex + relativeSelectionLength); }
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
                font = value;
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
        /// <summary>
        /// Selects all the text within this <see cref="TextField"/>.
        /// </summary>
        public void SelectAll()
        {
            caretIndex = text.Length;
            relativeSelectionLength = -text.Length;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            int height = font == null ? 0 : (int)Math.Ceiling(font.GetHeight());
            return new Size(padding.TotalX, padding.TotalY + height);
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
            AcquireKeyboardFocus();
            if (@event.ClickCount > 1) SelectAll();

            return true;
        }

        protected override bool OnKeyEvent(KeyEvent @event)
        {
            if (!@event.IsDown || @event.ModifierKeys != ModifierKeys.None) return true;

            switch (@event.Key)
            {
                case Key.Left:
                    if (HasSelection)
                    {
                        caretIndex = SelectionStartIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex > 0) --caretIndex;
                    break;

                case Key.Right:
                    if (HasSelection)
                    {
                        caretIndex = SelectionEndIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex < text.Length) ++caretIndex;
                    break;

                case Key.Home:
                    caretIndex = 0;
                    relativeSelectionLength = 0;
                    break;

                case Key.End:
                    caretIndex = text.Length;
                    relativeSelectionLength = 0;
                    break;

                case Key.Back:
                    if (HasSelection)
                    {
                        Text = text.Substring(0, SelectionStartIndex) + text.Substring(SelectionEndIndex);
                        caretIndex = SelectionStartIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex > 0)
                    {
                        Text = text.Substring(0, caretIndex - 1) + text.Substring(caretIndex);
                        --caretIndex;
                    }
                    break;

                case Key.Delete:
                    if (HasSelection)
                    {
                        Text = text.Substring(0, SelectionStartIndex) + text.Substring(SelectionEndIndex);
                        caretIndex = SelectionStartIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex < text.Length)
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
