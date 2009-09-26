using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Orion.Graphics
{
    class ClippedView : View
    {
        public ClippedView(Rectangle frame)
            : base(frame)
        { }

        protected override void Draw(GraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}
