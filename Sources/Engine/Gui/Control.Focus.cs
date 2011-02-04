using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    // This class part defines members relating to the keyboard focus and the mouse capture.
    partial class Control
    {
        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> currently has the keyboard focus.
        /// </summary>
        public bool HasKeyboardFocus
        {
            get { return manager != null && manager.KeyboardFocusedControl == this; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> currently has captured the mouse.
        /// </summary>
        public bool HasMouseCapture
        {
            get { return manager != null && manager.MouseCapturedControl == this; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Control"/> gains or loses keyboard focus.
        /// </summary>
        public event Action<Control> KeyboardFocusStateChanged;

        /// <summary>
        /// Raised when this <see cref="Control"/> gains or loses mouse capture.
        /// </summary>
        public event Action<Control> MouseCaptureStateChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Gives the keyboard focus to this <see cref="Control"/>.
        /// </summary>
        public void AcquireKeyboardFocus()
        {
            if (manager != null) manager.KeyboardFocusedControl = this;
        }

        /// <summary>
        /// Removes the keyboard focus from this <see cref="Control"/>.
        /// </summary>
        public void ReleaseKeyboardFocus()
        {
            if (HasKeyboardFocus) manager.KeyboardFocusedControl = null;
        }

        /// <summary>
        /// Gives the mouse capture to this <see cref="Control"/>.
        /// </summary>
        public void AcquireMouseCapture()
        {
            if (manager != null) manager.MouseCapturedControl = this;
        }

        /// <summary>
        /// Removes the mouse capture from this <see cref="Control"/>.
        /// </summary>
        public void ReleaseMouseCapture()
        {
            if (HasMouseCapture) manager.MouseCapturedControl = null;
        }

        /// <summary>
        /// Invoked by the <see cref="UIManager"/> to inform this <see cref="Control"/> that it has lost the keyboard focus.
        /// </summary>
        internal void HandleKeyboardFocusAcquisition()
        {
            OnKeyboardFocusAcquired();
            KeyboardFocusStateChanged.Raise(this);
        }

        /// <summary>
        /// Invoked by the <see cref="UIManager"/> to inform this <see cref="Control"/> that it has acquired the keyboard focus.
        /// </summary>
        internal void HandleKeyboardFocusLoss()
        {
            OnKeyboardFocusLost();
            KeyboardFocusStateChanged.Raise(this);
        }

        /// <summary>
        /// Invoked by the <see cref="UIManager"/> to inform this <see cref="Control"/> that it has lost the mouse capture.
        /// </summary>
        internal void HandleMouseCaptureAcquisition()
        {
            OnMouseCaptureAcquired();
            MouseCaptureStateChanged.Raise(this);
        }

        /// <summary>
        /// Invoked by the <see cref="UIManager"/> to inform this <see cref="Control"/> that it has acquired the mouse capture.
        /// </summary>
        internal void HandleMouseCaptureLoss()
        {
            OnMouseCaptureLost();
            MouseCaptureStateChanged.Raise(this);
        }

        /// <summary>
        /// Invoked when this <see cref="Control"/> acquires keyboard focus.
        /// </summary>
        protected virtual void OnKeyboardFocusAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses keyboard focus.
        /// </summary>
        protected virtual void OnKeyboardFocusLost() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> acquires the mouse capture.
        /// </summary>
        protected virtual void OnMouseCaptureAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses the mouse capture.
        /// </summary>
        protected virtual void OnMouseCaptureLost() { }
        #endregion
    }
}
