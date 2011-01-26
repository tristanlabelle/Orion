using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;
using Orion.Engine.Input;
using Key = OpenTK.Input.Key;
using SystemInformation = System.Windows.Forms.SystemInformation;

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
        private Func<char, bool> characterPredicate;
        private Font font;
        private int cachedFontHeight;
        private ColorRgba textColor = Colors.Black;
        private ColorRgba caretColor = Colors.Black;
        private int caretWidth = SystemInformation.CaretWidth;
        private TimeSpan caretBlinkDuration = TimeSpan.FromMilliseconds(SystemInformation.CaretBlinkTime);
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
                if (caretIndex > text.Length) caretIndex = text.Length;

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
        /// Accesses a delegate to a method which filters out entered characters.
        /// </summary>
        public Func<char, bool> CharacterPredicate
        {
            get { return characterPredicate; }
            set { characterPredicate = value; }
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
        /// Accesses the color of the caret.
        /// </summary>
        public ColorRgba CaretColor
        {
            get { return caretColor; }
            set { caretColor = value; }
        }

        /// <summary>
        /// Accesses the width of the caret, in pixels.
        /// </summary>
        public int CaretWidth
        {
            get { return caretWidth; }
            set
            {
                Argument.EnsurePositive(value, "CaretWidth");
                caretWidth = value;
            }
        }

        /// <summary>
        /// Accesses the duration of a caret blink cycle.
        /// </summary>
        public TimeSpan CaretBlinkDuration
        {
            get { return caretBlinkDuration; }
            set
            {
                if (value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("CaretBlinkDuration");
                caretBlinkDuration = value;
            }
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

        /// <summary>
        /// Gets the rectangle in which the text is drawn.
        /// </summary>
        public Region InnerRectangle
        {
            get { return Borders.ShrinkClamped(Rectangle, padding); }
        }

        private bool ShouldCaretBeDrawn
        {
            get
            {
                return Manager != null
                    && HasKeyboardFocus
                    && caretWidth > 0
                    && caretColor.A > 0
                    && Manager.Time.Ticks % (caretBlinkDuration.Ticks * 2) < caretBlinkDuration.Ticks;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a value indicating if a given character can be typed in this <see cref="TextField"/>.
        /// </summary>
        /// <param name="character">The character to be tested.</param>
        /// <returns><c>True</c> if the character is allowed, <c>false</c> if not.</returns>
        public bool IsAllowedCharacter(char character)
        {
            return !"\b\r\t\n".Contains(character)
                && !char.IsControl(character)
                && (characterPredicate == null || characterPredicate(character));
        }

        protected override Size MeasureSize(Size availableSize)
        {
            return new Size(padding.TotalX, padding.TotalY + cachedFontHeight);
        }

        protected internal override void Draw()
        {
            Region innerRectangle = InnerRectangle;
            if (innerRectangle.Area == 0) return;

            var options = new TextRenderingOptions
            {
                Font = font,
                Color = textColor,
                Origin = new Point(innerRectangle.MinX, innerRectangle.MinY)
            };
            Renderer.DrawText(text, ref options);

            if (ShouldCaretBeDrawn)
            {
                Size sizeBeforeCaret = Renderer.MeasureText(new Substring(text, 0, caretIndex), ref options);
                Region caretRectangle = new Region(
                    innerRectangle.MinX + sizeBeforeCaret.Width,
                    innerRectangle.MinY, caretWidth, innerRectangle.Height);
                Renderer.DrawRectangle(caretRectangle, caretColor);
            }
        }

        protected override void ArrangeChildren() { }

        protected override bool OnMouseButton(MouseEvent @event)
        {
            if (@event.Button == MouseButtons.Left && @event.IsPressed && isEditable)
            {
                PositionCaret(@event.Position);
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
                        text = text.Substring(0, caretIndex - 1) + text.Substring(caretIndex);
                        --caretIndex;
                        TextChanged.Raise(this);
                    }
                    break;

                case Key.Delete:
                    if (caretIndex < text.Length)
                    {
                        text = text.Substring(0, caretIndex) + text.Substring(caretIndex + 1);
                        TextChanged.Raise(this);
                    }
                    break;
            }

            return true;
        }

        protected override bool OnCharacterTyped(char character)
        {
            if (IsAllowedCharacter(character))
            {
                text = text.Insert(caretIndex, character);
                ++caretIndex;
                TextChanged.Raise(this);
            }

            return true;
        }

        private void PositionCaret(Point point)
        {
            Region innerRectangle = InnerRectangle;
            if (innerRectangle.Width == 0 || text.Length == 0)
            {
                caretIndex = text.Length;
                return;
            }

            int x = point.X - innerRectangle.MinX;

            var options = new TextRenderingOptions { Font = font };

            int minCharacterIndex = 0;
            int minCharacterX = 0;
            int exclusiveMaxCharacterIndex = text.Length;
            int exclusiveMaxCharacterX = Renderer.MeasureText(text, ref options).Width;
            while (exclusiveMaxCharacterIndex - minCharacterIndex > 1)
            {
                int index = (minCharacterIndex + exclusiveMaxCharacterIndex) / 2;
                int textWidth = Renderer.MeasureText(new Substring(text, 0, index), ref options).Width;

                if (x < textWidth)
                {
                    exclusiveMaxCharacterIndex = index;
                    exclusiveMaxCharacterX = textWidth;
                }
                else
                {
                    minCharacterIndex = index;
                    minCharacterX = textWidth;
                }
            }

            caretIndex = x - minCharacterX < exclusiveMaxCharacterX - x
                ? minCharacterIndex : exclusiveMaxCharacterIndex;
        }
        #endregion
    }
}
