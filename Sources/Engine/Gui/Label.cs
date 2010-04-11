using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Font = System.Drawing.Font;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A Label is a visible readonly text field.
    /// </summary>
    [DebuggerDisplay("{String} label")]
    public class Label : View
    {
        #region Fields
        private string text;
        private Font customFont;
        private ColorRgba color = Colors.Black;
        #endregion

        #region Constructors
        public Label(Rectangle frame, string text, Font customFont)
            : base(frame)
        {
            Argument.EnsureNotNull(text, "text");

            this.text = text;
            this.customFont = customFont;
        }

        public Label(Rectangle frame, string text)
            : this(frame, text, null) { }

        public Label(Rectangle frame)
            : this(frame, string.Empty) { }

        public Label(Rectangle frame, Text text)
            : this(frame, text.Value, text.Font) { }

        public Label(string text, Font customFont)
            : base(Rectangle.Unit)
        {
            Argument.EnsureNotNull(text, "text");

            this.text = text;
            this.customFont = customFont;

            base.Frame = TextObject.Frame;
            base.Bounds = base.Frame;
        }

        public Label(string text)
            : this(text, null) { }

        public Label()
            : this(string.Empty) { }

        public Label(Text text)
            : this(text.Value, text.Font) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses this label's text contents.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                Argument.EnsureNotNull(value, "Text");
                this.text = value;
            }
        }

        /// <summary>
        /// Accesses this label's text font.
        /// If <c>null</c>, the default font will be used.
        /// </summary>
        public Font CustomFont
        {
            get { return customFont; }
            set { customFont = value; }
        }

        /// <summary>
        /// Accesses this label's text color.
        /// </summary>
        public ColorRgba Color
        {
            get { return color; }
            set { color = ColorRgba.Clamp(value); }
        }

        public Text TextObject
        {
            get
            {
                return customFont == null
                    ? new Text(text)
                    : new Text(text, customFont);
            }
        }
        #endregion

        #region Methods
        protected internal override void Draw(GraphicsContext context)
        {
            context.Draw(TextObject, Bounds, color);
        }
        #endregion
    }
}
