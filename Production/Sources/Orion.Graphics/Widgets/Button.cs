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

        public Button(Rectangle frame, Label caption)
            : base(frame)
        {
            this.caption = caption;
			Children.Add(this.caption);
        }

        protected override void Draw()
        {
            Context.FillColor = Color.Blue;
            Context.StrokeColor = Color.Black;
            Context.Fill(Bounds);
            Context.Stroke(Bounds);
			
        }
    }
}
