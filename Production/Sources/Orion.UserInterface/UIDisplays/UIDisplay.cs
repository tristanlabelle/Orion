
namespace Orion.UserInterface
{
    public abstract class UIDisplay : Responder
    {
        public UIDisplay()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }

        public event GenericEventHandler<UIDisplay, RootView> Entered;
        public event GenericEventHandler<UIDisplay, RootView> Shadowed;

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
            GenericEventHandler<UIDisplay, RootView> handler = Entered;
            if (handler != null) handler(this, enterOn);
        }

        internal virtual void OnShadow(RootView hiddenOf)
        {
            GenericEventHandler<UIDisplay, RootView> handler = Shadowed;
            if (handler != null) handler(this, hiddenOf);
        }

        public override void Dispose()
        {
            Entered = null;
            Shadowed = null;
            base.Dispose();
        }
    }
}
