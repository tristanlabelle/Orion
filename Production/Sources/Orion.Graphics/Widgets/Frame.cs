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
        
        protected internal override bool OnMouseClick(MouseEventArgs args)
        {
            Console.WriteLine("Frame was clicked at ({0}, {1})!", args.X, args.Y);
            base.OnMouseClick(args);
            return false;
        }

        protected override void Draw()
        {
            context.FillColor = Color.White;
            context.StrokeColor = Color.Black;
            context.Fill(Bounds);
            context.Stroke(Bounds);
        }
    }
}
