
namespace Orion.UserInterface
{
    public abstract class UIDisplay : Responder
    {
        public UIDisplay()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }

        public new ViewChildrenCollection Children
        {
            get { return base.Children as ViewChildrenCollection; }
        }

        public new RootView Parent
        {
            get { return base.Parent as RootView; }
        }

        internal abstract void OnEnter(RootView enterOn);
        internal abstract void OnShadow(RootView hiddenOf);
    }
}
