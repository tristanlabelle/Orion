using System;
using Orion.Engine;

namespace Orion.UserInterface
{
    public abstract class UIDisplay : Responder
    {
        #region Constructors
        public UIDisplay()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="UIDisplay"/> is made active.
        /// </summary>
        public event Action<UIDisplay> Entered;

        /// <summary>
        /// Raised when this <see cref="UIDisplay"/> is made inactive because another one was pushed over it.
        /// </summary>
        public event Action<UIDisplay> Shadowed;
        #endregion

        #region Properties
        public new RootView Parent
        {
            get { return (RootView)base.Parent; }
        }

        public new RootView Root
        {
            get { return Parent; }
        }
        #endregion

        #region Methods
        protected internal virtual void OnEntered()
        {
            Entered.Raise(this);
        }

        protected internal virtual void OnShadowed()
        {
            Shadowed.Raise(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Entered = null;
                Shadowed = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
