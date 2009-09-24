using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics.Drawing;

namespace Orion.Graphics
{
    class ClippedView : View
    {
        public ClippedView(Rect frame)
            : base(frame)
        { }

        protected override void Draw(GraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}
