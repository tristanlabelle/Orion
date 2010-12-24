using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A button <see cref="Control"/> that can be clicked by the user.
    /// </summary>
    public sealed class Button : Control
    {
        #region Fields
        private readonly SingleChildCollection children;
        private Control content;
        private bool isEnabled = true;
        private bool isDown;
        #endregion

        #region Constructors
        public Button()
        {
            children = new SingleChildCollection(() => content, value => content = value);
            MinSize = new Size(30, 10);
        }

        public Button(string text, Borders padding)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            Content = new Label(text)
            {
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                Margin = padding
            };
        }

        public Button(string text)
            : this(text, new Borders(8)) { }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Button"/> gets clicked,
        /// either programatically, using the mouse or the keyboard.
        /// </summary>
        public event Action<Button> Clicked;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the content <see cref="Control"/> of this <see cref="Button"/>.
        /// </summary>
        public Control Content
        {
            get { return content; }
            set
            {
                if (value == content) return;

                if (content != null)
                {
                    AbandonChild(content);
                    content = null;
                }

                if (value != null)
                {
                    AdoptChild(value);
                    content = value;
                }

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="Button"/> is enabled (can be clicked by the user).
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value == isEnabled) return;

                isEnabled = value;
                if (!isEnabled) ReleaseKeyboardFocus();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Simulates a click from the user.
        /// </summary>
        public void Click()
        {
            if (Clicked != null) Clicked(this);
        }

        protected override ICollection<Control> GetChildren()
        {
            return children;
        }

        protected override Size MeasureWithoutMargin()
        {
            return (content == null ? Size.Zero : content.Measure());
        }

        protected internal override bool HandleKey(Keys key, Keys modifiers, bool pressed)
        {
            if (key == Keys.Enter && modifiers == Keys.None)
            {
                if (pressed) Click();
                return true;
            }

            return false;
        }

        protected internal override bool HandleMouseButton(MouseState state, MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.Left)
            {
                if (pressCount > 0)
                {
                    isDown = true;
                    AcquireKeyboardFocus();
                    AcquireMouseCapture();
                }
                else if (isDown)
                {
                    ReleaseMouseCapture();
                    isDown = false;

                    bool isMouseOver = HasAncestor(Manager.HoveredControl);
                    if (isMouseOver) Click();
                }

                return true;
            }

            return false;
        }

        protected internal override void OnMouseExited()
        {
            isDown = false;
        }
        #endregion
    }
}
