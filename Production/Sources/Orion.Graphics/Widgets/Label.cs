using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
//using OpenTK.Graphics;

using Orion.Graphics;
using Orion.Graphics.Drawing;

namespace Orion.Graphics.Widgets
{
    class Label : View
    {
        private string caption;

        public Label(Rectangle frame)
            : base(frame)
        { }

        public Label(Rectangle frame, string caption)
            : base(frame)
        {
            this.caption = caption;
        }

        protected override void Draw(GraphicsContext context)
        {
            context.DrawTextInView("bla", this);
        }
    }
}
