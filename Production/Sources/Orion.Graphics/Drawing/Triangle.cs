using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

namespace Orion.Graphics.Drawing
{
    class Triangle : IDrawable
    {
        public readonly Vector2 A;
        public readonly Vector2 B;
        public readonly Vector2 C;
        public readonly System.Drawing.Color Color;

        public Triangle(Vector2 a, Vector2 b, Vector2 c, System.Drawing.Color color)
        {
            A = a;
            B = b;
            C = c;
            Color = color;
        }

        public void Stroke()
        {
            GL.Begin(BeginMode.Lines);
            GL.Color3(Color);

            GL.Vertex2(A);
            GL.Vertex2(B);
            GL.Vertex2(C);

            GL.End();
        }

        public void Fill()
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color);

            GL.Vertex2(A);
            GL.Vertex2(B);
            GL.Vertex2(C);

            GL.End();
        }
    }
}
