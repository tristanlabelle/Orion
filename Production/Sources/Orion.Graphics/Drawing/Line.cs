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
    sealed class Line : IDrawable
    {
		public static Color DefaultColor = System.Drawing.Color.Black;
		public static int DefaultWidth = 1;
		
		public readonly Color Color;
		public readonly int Width;
		public Vector2 Start;
		public Vector2 End;
		
		public Line(Vector2 start, Vector2 end, Color color, int width)
		{
			Color = color;
			Width = width;
			Start = start;
			End = end;
		}
		
		public Line(Vector2 start, Vector2 end)
			: this(start, end, DefaultColor, DefaultWidth)
		{ }
		
		public Line(Vector2 start, Vector2 end, Color color)
			: this(start, end, color, DefaultWidth)
		{ }
		
		public void Stroke()
		{
			GL.Begin(BeginMode.Lines);
			GL.Color4(Color);
			GL.Vertex2(Start);
			GL.Vertex2(End);
			GL.End();
		}
		
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
