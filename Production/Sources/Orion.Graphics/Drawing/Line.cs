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

namespace Orion.Graphics.Drawing
{
    /// <summary>
    /// A Line object represents a drawable line. It has a color, a width and two coordinates.
    /// </summary>
    sealed class Line : IDrawable
    {
        /// <summary>
        /// The default color when one wasn't specified in the constructor.
        /// </summary>
		public static Color DefaultColor = System.Drawing.Color.Black;

        /// <summary>
        /// The default line width when one wasn't specified in the constructor.
        /// </summary>
		public static int DefaultWidth = 1;
		
        /// <summary>
        /// The color of the line.
        /// </summary>
		public readonly Color Color;

        /// <summary>
        /// The stroke width.
        /// </summary>
		public readonly int Width;

        /// <summary>
        /// The first point of the line.
        /// </summary>
		public Vector2 Start;

        /// <summary>
        /// The final point of the line.
        /// </summary>
		public Vector2 End;
		
        /// <summary>
        /// Constructs a new Line object.
        /// </summary>
        /// <param name="start">The start coordinate</param>
        /// <param name="end">The end coordinate</param>
        /// <param name="color">The color of the line</param>
        /// <param name="width">The stroke width of the line</param>
		public Line(Vector2 start, Vector2 end, Color color, int width)
		{
			Color = color;
			Width = width;
			Start = start;
			End = end;
		}

        /// <summary>
        /// Constructs a new Line object.
        /// </summary>
        /// <param name="start">The start coordinate</param>
        /// <param name="end">The end coordinate</param>
		public Line(Vector2 start, Vector2 end)
			: this(start, end, DefaultColor, DefaultWidth)
		{ }

        /// <summary>
        /// Constructs a new Line object.
        /// </summary>
        /// <param name="start">The start coordinate</param>
        /// <param name="end">The end coordinate</param>
        /// <param name="color">The color of the line</param>
		public Line(Vector2 start, Vector2 end, Color color)
			: this(start, end, color, DefaultWidth)
		{ }
		
        /// <summary>
        /// Makes a single stroke from the point to the end, using two vertexes and the basic
        /// Line mode. Has no respect to the Width field.
        /// </summary>
		public void Stroke()
		{
			GL.Begin(BeginMode.Lines);
			GL.Color4(Color);
			GL.Vertex2(Start);
			GL.Vertex2(End);
			GL.End();
		}
		
        /// <summary>
        /// Fills the line in respect to the Width field.
        /// </summary>
		public void Fill()
		{
			Vector2[] coords = new Vector2[2];
			
			Vector2 normalizedEnd = End;
			normalizedEnd.Sub(Start);
			
			double angle = Math.Atan2(normalizedEnd.Y, normalizedEnd.X) + Math.PI / 2;
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			coords[0] = new Vector2((float) (cos * Width), (float) (sin * Width));
			coords[1] = new Vector2((float) (-cos * Width), (float) (-sin * Width));
			
			GL.Begin(BeginMode.TriangleStrip);
			GL.Color4(Color);
			
			coords[0].Add(Start);
			coords[1].Add(Start);
			GL.Vertex2(coords[0]);
			GL.Vertex2(coords[1]);
			
			coords[0].Add(normalizedEnd);
			coords[1].Add(normalizedEnd);
			GL.Vertex2(coords[0]);
			GL.Vertex2(coords[1]);
			
			GL.End();
		}
    }
}
