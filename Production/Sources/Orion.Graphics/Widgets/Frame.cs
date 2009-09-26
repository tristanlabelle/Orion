using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics.Widgets
{
    class Frame : View
    {
        public Frame(Rectangle frame)
            : base(frame)
        { }

        protected override void Draw(GraphicsContext context)
        {
			context.FillColor = Color.White;
            context.StrokeColor = Color.Black;
			context.FillRect(Bounds);
			context.StrokeRect(Bounds);
        }
    }
}
