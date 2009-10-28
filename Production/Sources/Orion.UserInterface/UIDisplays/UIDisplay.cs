using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.UserInterface
{
    public abstract class UIDisplay : Responder
    {
        public UIDisplay()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }

        internal abstract void OnEnter(RootView enterOn);
        internal abstract void OnShadow(RootView hiddenOf);
    }
}
