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
            get { return base.Children as ViewChildrenCollection; }
        }

        public new RootView Parent
        {
            get { return base.Parent as RootView; }
        }

        internal virtual void OnEnter(RootView enterOn)
        {
            Action<UIDisplay, RootView> handler = Entered;
            if (handler != null) handler(this, enterOn);
        }

        internal virtual void OnShadow(RootView hiddenOf)
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
