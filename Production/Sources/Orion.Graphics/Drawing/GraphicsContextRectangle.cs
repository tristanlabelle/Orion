
using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
	public partial class GraphicsContext
	{
		public void FillRect(float x, float y, float a, float b)
		{
			FillRect(new Rect(x, y, a, b));
		}
		
		public void FillRect(Rect rectangle)
		{
			GL.Begin(BeginMode.Polygon);
			DrawRect(rectangle);
			GL.End();
		}
		
		public void StrokeRect(float x, float y, float width, float height)
		{
			StrokeRect(new Rect(x, y, width, height));
		}
		
		public void StrokeRect(Rect rectangle)
		{
			GL.Begin(BeginMode.LineStrip);
			DrawRect(rectangle);
			GL.End();
		}
		
		private void DrawRect(Rect rectangle)
		{
            Vector2 coords = rectangle.Position;
            GL.Vertex2(coords);
            coords.Y += rectangle.Size.Y;
            GL.Vertex2(coords);
            coords.X += rectangle.Size.X;
            GL.Vertex2(coords);
            coords.Y -= rectangle.Size.Y;
            GL.Vertex2(coords);
		}
	}
}
