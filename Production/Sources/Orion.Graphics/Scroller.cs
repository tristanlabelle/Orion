using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

using Orion.Geometry;
using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    class Scroller : View
    {
        private WorldView worldView;
        private Vector2 direction;
        private Rectangle worldBounds;

        public Scroller(Rectangle frame, WorldView worldView, Vector2 direction, Rectangle worldBounds)
            : base(frame)
        {
            this.worldView = worldView;
            this.direction = direction;
            this.worldBounds = worldBounds;
        }

        private bool checkViewBounds()
        {
            if(worldBounds.ContainsPoint(worldView.Bounds.Origin + direction) &&
                worldBounds.ContainsPoint(worldView.Bounds.Max + direction))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected internal override bool OnMouseMove(MouseEventArgs args)
        {
            if (checkViewBounds())
            {
                worldView.Bounds = worldView.Bounds.Translate(direction);
            }
            return true;
        }

        protected override void Draw()
        {
            /*context.FillColor = Color.Beige;
            context.Fill(Bounds);*/
        }
    }
}
