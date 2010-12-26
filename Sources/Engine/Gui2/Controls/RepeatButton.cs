using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MouseButtons = System.Windows.Forms.MouseButtons;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which looks like a button but generates events
    /// repeatedly if holded down.
    /// </summary>
    public sealed class RepeatButton : ContentControl
    {
        #region Fields
        private readonly Action<UIManager, TimeSpan> updatedEventHandler;
        private int clickCount;
        private TimeSpan timeBeforeRepeat;
        #endregion

        #region Constructors
        public RepeatButton()
        {
            updatedEventHandler = OnUpdated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="RepeatButton"/> gets pressed, or when it is hold.
        /// The first parameter is the source of the event while the second parameter is the repeat count.
        /// </summary>
        public event Action<RepeatButton, int> Clicked;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="RepeatButton"/> is currently down.
        /// </summary>
        public bool IsPressed
        {
            get { return clickCount > 0; }
        }

        public TimeSpan RepeatDelay
        {
            get { return TimeSpan.FromMilliseconds(SystemInformation.KeyboardDelay); }
        }

        public TimeSpan RepeatInterval
        {
            get { return TimeSpan.FromMilliseconds(SystemInformation.KeyboardSpeed); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Raises the <see cref="Clicked"/> event.
        /// </summary>
        public void Click()
        {
            Clicked.Raise(this, 1);
        }

        protected internal override bool OnMouseButton(MouseState state, MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.Left)
            {
                if (pressCount > 0)
                {
                    AcquireMouseCapture();
                    clickCount = 1;
                    Clicked.Raise(this, 1);
                    timeBeforeRepeat = RepeatDelay;
                }
                else if (clickCount > 0)
                {
                    clickCount = 0;
                    timeBeforeRepeat = TimeSpan.Zero;
                    ReleaseMouseCapture();
                }
            }

            return false;
        }

        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Updated -= updatedEventHandler;
            if (Manager != null) Manager.Updated += updatedEventHandler;
        }

        private void OnUpdated(UIManager sender, TimeSpan elapsedTime)
        {
            if (!IsPressed) return;

            timeBeforeRepeat -= elapsedTime;
            if (timeBeforeRepeat <= TimeSpan.Zero)
            {
                ++clickCount;
                Clicked.Raise(this, clickCount);
                timeBeforeRepeat = RepeatInterval;
            }
        }
        #endregion
    }
}
