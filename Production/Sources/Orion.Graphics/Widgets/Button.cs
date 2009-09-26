using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics.Widgets
{
    class Button : View
    {
        private Label caption;

        public Button(Rectangle frame)
            : base(frame)
        { }

        public Button(Rectangle frame, Label caption)
            : base(frame)
        {
            this.caption = caption;
        }

        protected override void Draw(GraphicsContext context)
        {
            context.FillColor = Color.Blue;
            context.StrokeColor = Color.Black;
            context.Fill(Bounds);
            context.Stroke(Bounds);
        }
    }
}
