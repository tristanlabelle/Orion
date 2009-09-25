using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics.Drawing
{
	public partial class GraphicsContext
	{
        /// <summary>
        /// Fills a rectangle with a given origin, width and height.
        /// </summary>
        /// <param name="x">The origin abscissa of the rectangle</param>
        /// <param name="y">The origin ordinate of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
		public void FillRect(float x, float y, float width, float height)
		{
			FillRect(new Rectangle(x, y, width, height));
		}
		
        /// <summary>
        /// Fills the rectangle represented by the <see href="Rectangle"/> structure.
        /// </summary>
        /// <param name="rectangle">The <see href="Rectangle"/> structure to fill onscreen</param>
		public void FillRect(Rectangle rectangle)
		{
			GL.Begin(BeginMode.Polygon);
			DrawRect(rectangle);
			GL.End();
		}

        /// <summary>
        /// Strokes the outline of a rectangle with a given origin, width and height.
        /// </summary>
        /// <param name="x">The origin abscissa of the rectangle</param>
        /// <param name="y">The origin ordinate of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
		public void StrokeRect(float x, float y, float width, float height)
		{
			StrokeRect(new Rectangle(x, y, width, height));
		}

        /// <summary>
        /// Strokes the outline of the rectangle represented by the <see href="Rectangle"/> structure.
        /// </summary>
        /// <param name="rectangle">The <see href="Rectangle"/> structure to fill onscreen</param>
		public void StrokeRect(Rectangle rectangle)
		{
			GL.Begin(BeginMode.LineStrip);
			DrawRect(rectangle);
			GL.End();
		}
		
		private void DrawRect(Rectangle rectangle)
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
