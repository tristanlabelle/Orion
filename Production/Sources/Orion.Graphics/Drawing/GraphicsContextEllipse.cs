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
        #region Circles

        /// <summary>
        /// Fills a circle centered at a given origin, with a given radius.
        /// </summary>
        /// <param name="x">The origin abscissa of the circle</param>
        /// <param name="y">The origin ordinate of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        public void FillCircle(float x, float y, float radius)
		{
			FillCircle(new Vector2(x, y), radius);
		}
		
        /// <summary>
        /// Fills a circle centered at a given point, with a given radius.
        /// </summary>
        /// <param name="center">The origin of the circle</param>
        /// <param name="radius">The radius of the circle</param>
		public void FillCircle(Vector2 center, float radius)
		{
			FillEllipse(center, new Vector2(radius, radius));
        }

        /// <summary>
        /// Strokes the outline of a circle centered at a given origin, with a given radius.
        /// </summary>
        /// <param name="x">The origin abscissa of the circle</param>
        /// <param name="y">The origin ordinate of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        public void StrokeCircle(float x, float y, float radius)
        {
            StrokeCircle(new Vector2(x, y), radius);
        }

        /// <summary>
        /// Strokes the outline of a circle centered at a given point, with a given radius.
        /// </summary>
        /// <param name="center">The origin of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        public void StrokeCircle(Vector2 center, float radius)
        {
            StrokeEllipse(center, new Vector2(radius, radius));
        }
        #endregion

        #region Ellipses

        /// <summary>
        /// Fills an ellipse centered at a given origin, with two independant radii (X radius and Y radius).
        /// </summary>
        /// <param name="x">The origin abscissa of the circle</param>
        /// <param name="y">The origin ordinate of the circle</param>
        /// <param name="rX">The radius of the ellipse on the X axis</param>
        /// <param name="rY">The raidus of the ellipse on the Y axis</param>
        public void FillEllipse(float x, float y, float rX, float rY)
		{
			FillEllipse(new Vector2(x, y), new Vector2(rX, rY));
		}
		
        /// <summary>
        /// Fills an ellipse centered at a given point, with a vector representing two independant radii.
        /// </summary>
        /// <param name="center">The center of the ellipse</param>
        /// <param name="radii">The vector containing the X and Y radii</param>
		public void FillEllipse(Vector2 center, Vector2 radii)
		{
			GL.Begin(BeginMode.Polygon);
			DrawEllipse(center, radii);
			GL.End();
		}

        /// <summary>
        /// Strokes the outline of an ellipse centered at a given origin, with two independant radii (X radius and Y radius).
        /// </summary>
        /// <param name="x">The origin abscissa of the circle</param>
        /// <param name="y">The origin ordinate of the circle</param>
        /// <param name="rX">The radius of the ellipse on the X axis</param>
        /// <param name="rY">The raidus of the ellipse on the Y axis</param>
		public void StrokeEllipse(float x, float y, float rX, float rY)
		{
			StrokeEllipse(new Vector2(x, y), new Vector2(rX, rY));
		}

        /// <summary>
        /// Strokes the outline of an ellipse centered at a given point, with a vector representing two independant radii.
        /// </summary>
        /// <param name="center">The center of the ellipse</param>
        /// <param name="radii">The vector containing the X and Y radii</param>
		public void StrokeEllipse(Vector2 center, Vector2 radii)
		{
			GL.Begin(BeginMode.LineStrip);
			DrawEllipse(center, radii);
			GL.End();
        }
        #endregion

        private const int EllipsePoints = 36;

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
