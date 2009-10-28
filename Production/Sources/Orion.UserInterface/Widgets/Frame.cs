using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;
using Orion.Graphics;

using Color = System.Drawing.Color;

namespace Orion.UserInterface.Widgets
{
    class Frame : View
    {
        public Frame(Rectangle frame)
            : base(frame)
        { }

        protected internal override void Draw(GraphicsContext context)
        {
            context.FillColor = Color.White;
            context.StrokeColor = Color.Black;
            context.Fill(Bounds);
            context.Stroke(Bounds);
        }
    }
}
