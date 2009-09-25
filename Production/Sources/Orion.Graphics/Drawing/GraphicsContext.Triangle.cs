using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
	public partial class GraphicsContext
	{
        /// <summary>
        ///  Fills a triangle shape using three control points.
        /// </summary>
        /// <param name="aX">Abscissa of the first control point</param>
        /// <param name="aY">Ordinate of the first control point</param>
        /// <param name="bX">Abscissa of the second control point</param>
        /// <param name="bY">Ordinate of the second control point</param>
        /// <param name="cX">Abscissa of the third control point</param>
        /// <param name="cY">Ordinate of the third control point</param>
		public void FillTriangle(float aX, float aY, float bX, float bY, float cX, float cY)
		{
			FillTriangle(new Vector2(aX, aY), new Vector2(bX, bY), new Vector2(cX, cY));
		}
		
        /// <summary>
        /// Fills a triangle shape using three control point vectors.
        /// </summary>
        /// <param name="a">First control point</param>
        /// <param name="b">Second control point</param>
        /// <param name="c">Third control point</param>
		public void FillTriangle(Vector2 a, Vector2 b, Vector2 c)
		{
			GL.Begin(BeginMode.Triangles);
			DrawTriangle(a, b, c);
			GL.End();
		}
		
        /// <summary>
        /// Strokes the outline of a triangle using three control points.
        /// </summary>
        /// <param name="aX">Abscissa of the first control point</param>
        /// <param name="aY">Ordinate of the first control point</param>
        /// <param name="bX">Abscissa of the second control point</param>
        /// <param name="bY">Ordinate of the second control point</param>
        /// <param name="cX">Abscissa of the third control point</param>
        /// <param name="cY">Ordinate of the third control point</param>
		public void StrokeTriangle(float aX, float aY, float bX, float bY, float cX, float cY)
		{
			StrokeTriangle(new Vector2(aX, aY), new Vector2(bX, bY), new Vector2(cX, cY));
		}

        /// <summary>
        /// Strokes the outline of a triangle shape using three control point vectors.
        /// </summary>
        /// <param name="a">First control point</param>
        /// <param name="b">Second control point</param>
        /// <param name="c">Third control point</param>
		public void StrokeTriangle(Vector2 a, Vector2 b, Vector2 c)
		{
			GL.Begin(BeginMode.LineLoop);
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
