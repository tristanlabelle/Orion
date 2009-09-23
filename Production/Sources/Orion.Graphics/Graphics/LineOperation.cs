using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Math;
using OpenTK.Input;
using OpenTK.Platform;

namespace Orion.Graphics.Graphics
{
    sealed class LineOperation : DrawingOperation
    {
        private readonly Color color;
        private readonly Vector2 start;
        private readonly Vector2 end;
        private readonly float width;

        public LineOperation(Vector2 start, Vector2 end, float width, Color color)
            : base()
        {
            this.start = start;
            this.end = end;
            this.width = width;
            this.color = color;
        }

        /*private void Flatten(Rect baseSystem, Rect finalSystem)
        {
            Vector2 scale = new Vector2(baseSystem.Size.X / finalSystem.Size.X, baseSystem.Size.Y / finalSystem.Size.Y);
            
            flattenedStart = start;
            flattenedStart.Sub(baseSystem.Position);
            flattenedStart.Scale(ref scale);

            flattenedEnd = end;
            flattenedEnd.Sub(baseSystem.Position);
            flattenedEnd.Scale(ref scale);
            flattened = true;
        }*/

        public void Draw(Rect baseSystem, Rect finalSystem)
        {
            GL.PushMatrix();

            Vector2 translate = start;
            translate.Sub(baseSystem.Position);

            Vector2 rotate = end;
            end.Sub(start);
            float angle = (float) (Math.Atan2(rotate.Y, rotate.X) + Math.PI);
            
            GL.Translate(-translate.X, -translate.Y, 0);
            GL.Scale(baseSystem.Size.X / finalSystem.Size.X, baseSystem.Size.Y / finalSystem.Size.Y, 1);
            GL.Rotate(angle, 0, 0, 1);

            double xDelta = start.X - end.X;
            double yDelta = start.Y - end.Y;
            double length = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);

            GL.Color4(color);

            float halfWidth = width / 2;
            GL.Vertex2(0, -halfWidth);

            Vector2 normalizedEnd = end;

            GL.PopMatrix();
        }
    }
}
