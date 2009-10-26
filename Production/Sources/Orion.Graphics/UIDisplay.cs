using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Graphics
{
    public abstract class UIDisplay : View
    {
        public UIDisplay()
            : base(RootView.ContentsBounds)
        { }

        internal abstract void OnEnter(RootView enterOn);
        internal abstract void OnShadow(RootView hiddenOf);

        protected override sealed void Draw()
        { }
    }
}
