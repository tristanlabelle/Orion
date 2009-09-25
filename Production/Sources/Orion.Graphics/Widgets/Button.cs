using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Orion.Graphics;
using Orion.Graphics.Drawing;

namespace Orion.Graphics.Widgets
{
    class Button : View
    {
        private Label caption;

        public Button(Rect frame)
            : base(frame)
        { }

        public Button(Rect frame, Label caption)
            : base(frame)
        {
            this.caption = caption;
        }

        protected override void Draw(GraphicsContext context)
        {
            context.Color = Color.Blue;
            context.FillRect(Bounds);
            context.Color = Color.Black;
            context.StrokeRect(Bounds);
        }
    }
}
