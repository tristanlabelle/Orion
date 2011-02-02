using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract partial class Control
    {
        #region Fields
        private object tag;
        #endregion

        #region Constructors
        protected Control()
        {
            manager = this as UIManager;
        }
        #endregion

        #region Properties
        #region Focus
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

        /// <summary>
        /// Accesses the tag of this <see cref="Control"/>, which is a user data object associated with it.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        #endregion

        #region Methods
        #region Focus
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
        /// Invoked when this <see cref="Control"/> acquires keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusLost() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> acquires the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureLost() { }
        #endregion
        #endregion
    }
}
