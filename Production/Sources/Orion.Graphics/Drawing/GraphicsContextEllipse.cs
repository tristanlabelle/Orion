using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
	partial class GraphicsContext
	{
		private const int EllipsePoints = 36;
		
		public void FillCircle(float x, float y, float radius)
		{
			FillCircle(new Vector2(x, y), radius);
		}
		
		public void FillCircle(Vector2 center, float radius)
		{
			FillEllipse(center, new Vector2(radius, radius));
		}
		
		public void FillEllipse(float x, float y, float rX, float rY)
		{
			FillEllipse(new Vector2(x, y), new Vector2(rX, rY));
		}
		
		public void FillEllipse(Vector2 center, Vector2 radii)
		{
			GL.Begin(BeginMode.Polygon);
			DrawEllipse(center, radii);
			GL.End();
		}
		
		public void StrokeCircle(float x, float y, float radius)
		{
			StrokeCircle(new Vector2(x, y), radius);
		}
		
		public void StrokeCircle(Vector2 center, float radius)
		{
			StrokeEllipse(center, new Vector2(radius, radius));
		}
		
		public void StrokeEllipse(float x, float y, float rX, float rY)
		{
			StrokeEllipse(new Vector2(x, y), new Vector2(rX, rY));
		}
		
		public void StrokeEllipse(Vector2 center, Vector2 radii)
		{
			GL.Begin(BeginMode.LineStrip);
			DrawEllipse(center, radii);
			GL.End();
		}
		
		private void DrawEllipse(Vector2 center, Vector2 radii)
		{
			for(int i = 0; i < EllipsePoints; i++)
			{
				double alpha = i * (360f / EllipsePoints) * (Math.PI / 180);
				GL.Vertex2(center.X + radii.X * Math.Cos(alpha), center.Y + radii.Y * Math.Sin(alpha));
			}
		}
	}
}
