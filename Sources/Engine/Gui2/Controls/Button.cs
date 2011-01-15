using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using Key = OpenTK.Input.Key;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A button <see cref="Control"/> that can be clicked by the user.
    /// </summary>
    public class Button : ContentControl
    {
        #region Fields
        /// <summary>
        /// The mouse button that is pressing this button, if any.
        /// </summary>
        private MouseButtons pressingButton;
        private bool acquireKeyboardFocusWhenPressed = true;
        private MouseButtons clickingButtons = MouseButtons.Left;
        #endregion

        #region Constructors
        public Button() { }

        public Button(string text)
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
        /// Accesses a value indicating which mouse buttons can be used to click the button, as flags.
        /// </summary>
        public MouseButtons ClickingButtons
        {
            get { return clickingButtons; }
            set { clickingButtons = value; }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="Button"/>
        /// should acquire the keyboard focus when a mouse button presses it.
        /// </summary>
        public bool AcquireKeyboardFocusWhenPressed
        {
            get { return acquireKeyboardFocusWhenPressed; }
            set { acquireKeyboardFocusWhenPressed = value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Button"/> is currently down.
        /// </summary>
        public bool IsDown
        {
            get { return pressingButton != MouseButtons.None; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Simulates a click from the user.
        /// </summary>
        public void Click()
        {
            OnClicked(MouseButtons.None);
        }

        protected virtual void OnClicked(MouseButtons button)
        {
            Clicked.Raise(this, button);
        }

        protected override bool OnKeyEvent(KeyEvent @event)
        {
            if (@event.Key == Key.Enter && @event.ModifierKeys == ModifierKeys.None)
            {
                if (@event.IsDown) Click();
                return true;
            }

            return false;
        }

        protected override bool OnMouseButton(MouseEvent @event)
        {
            if ((ClickingButtons & @event.Button) != 0)
            {
                if (@event.IsPressed)
                {
                    pressingButton = @event.Button;
                    if (acquireKeyboardFocusWhenPressed) AcquireKeyboardFocus();
                    AcquireMouseCapture();
                }
                else if (pressingButton != MouseButtons.None)
                {
                    ReleaseMouseCapture();
                    pressingButton = MouseButtons.None;

                    if (IsUnderMouse) OnClicked(@event.Button);
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
