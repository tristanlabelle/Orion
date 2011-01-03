﻿using System;
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

        /// <summary>
        /// The mouse button that is pressing this button, if any.
        /// </summary>
        private MouseButtons pressingButton;
        private MouseButtons clickingButtons = MouseButtons.Left;
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
        /// The argument specifies the mouse button used to click the button.
        /// </summary>
        public event Action<Button, MouseButtons> Clicked;
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
            get { return pressingButton != MouseButtons.None; }
        }

        /// <summary>
        /// Accesses a value indicating which mouse buttons can be used to click the button, as flags.
        /// </summary>
        public MouseButtons ClickingButtons
        {
            get { return clickingButtons; }
            set { clickingButtons = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Simulates a click from the user.
        /// </summary>
        public void Click()
        {
            if (Clicked != null) Clicked(this, MouseButtons.None);
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
            if ((ClickingButtons & button) != 0)
            {
                if (pressCount > 0)
                {
                    pressingButton = button;
                    AcquireKeyboardFocus();
                    AcquireMouseCapture();
                }
                else if (pressingButton != MouseButtons.None)
                {
                    ReleaseMouseCapture();
                    pressingButton = MouseButtons.None;

                    if (IsUnderMouse) Clicked.Raise(this, button);
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
