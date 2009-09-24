using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

namespace Orion.Graphics.Drawing
{
    class Rectangle : IDrawable
    {
        public readonly Rect Zone;
        public readonly System.Drawing.Color Color;

        public Rectangle(Rect rect, System.Drawing.Color color)
        {
            Zone = rect;
            Color = color;
        }

        public void Fill()
        {
            GL.Begin(BeginMode.Polygon);
            Render();
            GL.End();
        }

        public void Stroke()
        {
            GL.Begin(BeginMode.Lines);
            Render();
            GL.End();
        }

        private void Render()
        {
            Vector2 coords = Zone.Position;
            GL.Vertex2(coords);
            coords.Y += Zone.Size.Y;
            GL.Vertex2(coords);
            coords.X += Zone.Size.X;
            GL.Vertex2(coords);
            coords.Y -= Zone.Size.Y;
            GL.Vertex2(coords);
        }
    }
}
