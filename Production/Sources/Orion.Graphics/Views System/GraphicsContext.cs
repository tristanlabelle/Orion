using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

using Orion.Geometry;

using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace Orion.Graphics
{
	/// <summary>
	/// Defines possible stroke styles for the Stroke methods of a GraphicsContext.
	/// </summary>
    public enum StrokeStyle
    {
		/// <summary>
		/// A full, solid line 
		/// </summary>
        Solid = 0xFFFF,
		
		/// <summary>
		/// A dashed line 
		/// </summary>
        Dashed = 0x00FF,
		
		/// <summary>
		/// A dotted line 
		/// </summary>
        Dotted = 0xAAAA,
		
		/// <summary>
		/// A line whose stroke alternates between a dot and a dash 
		/// </summary>
        DotDash = 0x1C47
    }

    /// <summary>
    /// Represents a space in which it is possible to draw. Methods to fill and stroke shapes are supplied.
    /// </summary>
    public sealed class GraphicsContext
    {
        #region Instance
        #region Fields
        private Rectangle coordinateSystem;
        private Color fillColor = Color.White;
        private Color strokeColor = Color.Black;
        private StrokeStyle strokeStyle = StrokeStyle.Solid;
        private Font font;
        private TextPrinter printer;
        private bool readyForDrawing;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="GraphicsContext"/> object with the given bounds for its local coordinate system. 
        /// </summary>
        /// <param name="bounds">
        /// A <see cref="Rectangle"/> defining the local coordinate system.
        /// </param>
        internal GraphicsContext(Rectangle bounds)
        {
            coordinateSystem = bounds;
            strokeStyle = StrokeStyle.Solid;
            printer = new TextPrinter();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the bounds of the local coordinate system. 
        /// </summary>
        public Rectangle CoordinateSystem
        {
            get { return coordinateSystem; }
            set { coordinateSystem = value; }
        }

        /// <summary>
        /// Accesses the <see cref="Color"/> currently used to fill shapes.
        /// </summary>
        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        /// <summary>
        /// Accesses the <see cref="Color"/> currently used to stroke shape outlines.
        /// </summary>
        public Color StrokeColor
        {
            get { return strokeColor; }
            set { strokeColor = value; }
        }

        /// <summary>
        /// Accesses the <see cref="StrokeStyle"/> currently used to stroke shape outlines. (Currently has no effect.)
        /// </summary>
        public StrokeStyle StrokeStyle
        {
            get { return strokeStyle; }
            set 
            { 
                strokeStyle = value;
                CommitStrokeStyle();
            }
        }

        /// <summary>
        /// Accesses the <see cref="Font"/> currently used to render text.
        /// </summary>
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }
        #endregion

        #region Methods
        #region Drawing
        #region Ellipses (and implicitly converted circles)
        /// <summary>
        /// Fills a given <see cref="Ellipse"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be filled.</param>
        /// <param name="vertexCount">The number of vertices to use to approximate the shape.</param>
        public void Fill(Ellipse ellipse, int vertexCount)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Polygon);
            DrawVertices(ellipse, vertexCount);
            GL.End();
        }

        /// <summary>
        /// Fills a given <see cref="Ellipse"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be filled.</param>
        public void Fill(Ellipse ellipse)
        {
            Fill(ellipse, DefaultCircleVertexCount);
        }

        /// <summary>
        /// Strokes the outline of a given <see cref="Ellipse"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be strokes.</param>
        /// <param name="vertexCount">The number of vertices to use to approximate the shape.</param>
        public void Stroke(Ellipse ellipse, int vertexCount)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(ellipse, vertexCount);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a given <see cref="Ellipse"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be strokes.</param>
        public void Stroke(Ellipse ellipse)
        {
            Stroke(ellipse, DefaultCircleVertexCount);
        }

        private void DrawVertices(Ellipse ellipse, int vertexCount)
        {
            if(!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");
            Argument.EnsureStrictlyPositive(vertexCount, "vertexCount");

            double angleIncrement = Math.PI * 2 / vertexCount;
            for (int i = 0; i < vertexCount; i++)
            {
                double angle = angleIncrement * i;
                AddVertex(ellipse.Center.X + ellipse.Radii.X * Math.Cos(angle),
                    ellipse.Center.Y + ellipse.Radii.Y * Math.Sin(angle));
            }
        }
        #endregion

        #region Rectangles
        /// <summary>
        /// Fills a <see cref="Rectangle"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to fill with color.</param>
        public void Fill(Rectangle rectangle)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Polygon);
            DrawVertices(rectangle);
            GL.End();
        }



        /// <summary>
        /// Strokes the outline of a <see cref="Rectangle"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to stroke.</param>
        public void Stroke(Rectangle rectangle)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(rectangle);
            GL.End();
        }

        private void DrawVertices(Rectangle rectangle)
        {
            if(!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");
            AddVertex(rectangle.X, rectangle.Y);
            AddVertex(rectangle.X, rectangle.MaxY);
            AddVertex(rectangle.MaxX, rectangle.MaxY);
            AddVertex(rectangle.MaxX, rectangle.Y);
        }
        #endregion

        #region Triangles
        /// <summary>
        /// Fills a <see cref="Triangle"/> shape using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be filled.</param>
        public void Fill(Triangle triangle)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Triangles);
            DrawVertices(triangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Triangle"/> shape using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be stroked.</param>
        public void Stroke(Triangle triangle)
        {
            CommitStrokeStyle();
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(triangle);
            GL.End();
        }

        private void DrawVertices(Triangle triangle)
        {
            if(!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");
            AddVertex(triangle.Vertex1);
            AddVertex(triangle.Vertex2);
            AddVertex(triangle.Vertex3);
        }
        #endregion

        #region Text

        /// <summary>
        /// Printes text, using this context's defined font and color, to the view at the origin coordinates
        /// </summary>
        /// <param name="text">The text to print</param>
        public void DrawText(string text)
        {
            printer.Print(text, font, fillColor);
        }

		/// <summary>
        /// Prints text, using this context's defined font and color, to the view at specified coordinates. 
		/// </summary>
		/// <param name="text">The <see cref="System.String"/> to print</param>
		/// <param name="position">The position at which to print the string</param>
        public void DrawText(string text, Vector2 position)
        {
            GL.Translate(position.X, position.Y, 0);
            printer.Print(text, font, fillColor);
            GL.Translate(-position.X, -position.Y, 0);
        }

        #endregion

        private void AddVertex(double x, double y)
        {
            AddVertex(new Vector2((float)x, (float)y));
        }

        private void AddVertex(Vector2 position)
        {
            GL.Vertex2(position);
        }
        #endregion

        #region Non-Public
        /// <summary>
        /// Commits any changes to <see cref="FillColor"/> to OpenGL.
        /// </summary>
        private void CommitFillColor()
        {
            if (fillColor.A < 255)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
                GL.Disable(EnableCap.Blend);

            GL.Color4(fillColor.R, fillColor.G, fillColor.B, fillColor.A);
        }

        /// <summary>
        /// Commits any changes to <see cref="StrokeColor"/> to OpenGL.
        /// </summary>
        private void CommitStrokeColor()
        {
            GL.Color4(strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A);
        }

        private void CommitStrokeStyle()
        {
            GL.LineStipple(1, (short)strokeStyle);
        }

        internal void SetUpGLContext(Rectangle parentSystem)
        {
            GL.PushMatrix();

            GL.Translate(parentSystem.Origin.X, parentSystem.Origin.Y, 0);
            GL.Scale(parentSystem.Width / CoordinateSystem.Width, parentSystem.Height / CoordinateSystem.Height, 1);
            GL.Translate(-CoordinateSystem.Origin.X, -CoordinateSystem.Origin.Y, 0);
            
            readyForDrawing = true;
        }

        internal void RestoreGLContext()
        {
            GL.PopMatrix();
            
            readyForDrawing = false;
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        private const int DefaultCircleVertexCount = 32;
        #endregion
        #endregion
    }
}
