using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Orion.Graphics.Drawing;

namespace Orion.Graphics.Widgets
{
    class Frame : View
    {
        public Frame(Rect frame)
            : base(frame)
        { }

        protected override void Draw(Orion.Graphics.Drawing.GraphicsContext context)
        {
			context.Color = Color.White;
			context.FillRect(Bounds);
			context.Color = Color.Black;
			context.StrokeRect(Bounds);
        }
    }
}
