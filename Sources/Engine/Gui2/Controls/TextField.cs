using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;

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

        protected override Size MeasureSize()
        {
            return Size.Zero;
        }

        protected override void ArrangeChildren() { }

        protected internal override bool OnMouseButton(MouseState state, MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.Left && pressCount > 0 && isEditable)
            {
                AcquireKeyboardFocus();
                if (pressCount > 1) SelectAll();
                return true;
            }

            return false;
        }

        protected internal override bool OnKey(Keys key, Keys modifiers, bool pressed)
        {
            switch (key | modifiers)
            {
                case Keys.Left:
                    if (!pressed) return true;
                    if (HasSelection)
                    {
                        caretIndex = SelectionStartIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex > 0) --caretIndex;
                    return true;

                case Keys.Right:
                    if (!pressed) return true;
                    if (HasSelection)
                    {
                        caretIndex = SelectionEndIndex;
                        relativeSelectionLength = 0;
                    }
                    else if (caretIndex < text.Length) ++caretIndex;
                    return true;

                case Keys.Home:
                    if (!pressed) return true;
                    caretIndex = 0;
                    relativeSelectionLength = 0;
                    return true;

                case Keys.End:
                    if (!pressed) return true;
                    caretIndex = text.Length;
                    relativeSelectionLength = 0;
                    return true;

                case Keys.Back:
                    if (!pressed) return true;
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
                    return true;

                case Keys.Delete:
                    if (!pressed) return true;
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
                    return true;

                default:
                    return false;
            }
        }

        protected internal override bool OnCharacter(char character)
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
