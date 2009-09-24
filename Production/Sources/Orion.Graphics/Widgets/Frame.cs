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
            var rect = new Orion.Graphics.Drawing.Rectangle(Bounds, Color.White);
            context.Fill(rect);
            rect = new Orion.Graphics.Drawing.Rectangle(Bounds, Color.Black);
            context.Stroke(rect);
        }
    }
}
