using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
	public partial class GraphicsContext
	{
		public void FillTriangle(float aX, float aY, float bX, float bY, float cX, float cY)
		{
			FillTriangle(new Vector2(aX, aY), new Vector2(bX, bY), new Vector2(cX, cY));
		}
		
		public void FillTriangle(Vector2 a, Vector2 b, Vector2 c)
		{
			GL.Begin(BeginMode.Triangles);
			DrawTriangle(a, b, c);
			GL.End();
		}
		
		public void StrokeTriangle(float aX, float aY, float bX, float bY, float cX, float cY)
		{
			StrokeTriangle(new Vector2(aX, aY), new Vector2(bX, bY), new Vector2(cX, cY));
		}
		
		public void StrokeTriangle(Vector2 a, Vector2 b, Vector2 c)
		{
			GL.Begin(BeginMode.LineStrip);
			DrawTriangle(a, b, c);
			GL.End();
		}
		
		private void DrawTriangle(Vector2 a, Vector2 b, Vector2 c)
		{
			GL.Vertex2(a);
			GL.Vertex2(b);
			GL.Vertex2(c);
		}
	}
}
