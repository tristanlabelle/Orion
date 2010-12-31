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
    public sealed class Button : ContentControl
    {
        #region Fields
        private bool isEnabled = true;
        private bool isDown;
        #endregion

        #region Constructors
        public Button()
        {
            Padding = new Borders(10, 6);
            MinSize = new Size(30, 10);
        }

        public Button(string text)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            Content = new Label(text)
            {
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center
            };
        }
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

        /// <summary>
        /// Gets a value indicating if this <see cref="Button"/> is currently down.
        /// </summary>
        public bool IsDown
        {
            get { return isDown; }
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

        protected override bool OnKey(Keys key, Keys modifiers, bool pressed)
        {
            if (key == Keys.Enter && modifiers == Keys.None)
            {
                if (pressed) Click();
                return true;
            }

            return false;
        }

        protected override bool OnMouseButton(MouseState state, MouseButtons button, int pressCount)
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

                    if (IsUnderMouse) Click();
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
