using System;

namespace Orion.UserInterface
{
    public abstract class UIDisplay : Responder
    {
        public UIDisplay()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }

        public event Action<UIDisplay, RootView> Entered;
        public event Action<UIDisplay, RootView> Shadowed;

        public new ViewChildrenCollection Children
        {
            get { return (ViewChildrenCollection)base.Children; }
        }

        public new RootView Parent
        {
            get { return (RootView)base.Parent; }
        }

        internal virtual void OnEntered(RootView enterOn)
        {
            Action<UIDisplay, RootView> handler = Entered;
            if (handler != null) handler(this, enterOn);
        }

        internal virtual void OnShadowed(RootView hiddenOf)
        {
            Action<UIDisplay, RootView> handler = Shadowed;
            if (handler != null) handler(this, hiddenOf);
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
    }
}
