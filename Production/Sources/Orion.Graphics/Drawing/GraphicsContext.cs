using System;
using System.Collections.Generic;
using RectangleF = System.Drawing.RectangleF;

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
        private Font font = new Font("Consolas", 12);
        private bool readyForDrawing;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="GraphicsContext"/> object with the given bounds for its local coordinate system. 
        /// </summary>
        /// <param name="bounds">
        /// A <see cref="Rectangle"/> defining the local coordinate system.
        /// </param>
        public GraphicsContext(Rectangle bounds)
        {
            coordinateSystem = bounds;
            strokeStyle = StrokeStyle.Solid;
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
        #region OpenGL Context

        public void SetUpGLContext(Rectangle parentSystem)
        {
            GL.PushMatrix();

            GL.Translate(parentSystem.Min.X, parentSystem.Min.Y, 0);
            GL.Scale(parentSystem.Width / CoordinateSystem.Width, parentSystem.Height / CoordinateSystem.Height, 1);
            GL.Translate(-CoordinateSystem.Min.X, -CoordinateSystem.Min.Y, 0);

            readyForDrawing = true;
        }

        public void RestoreGLContext()
        {
            GL.PopMatrix();

            readyForDrawing = false;
        }
        #endregion

        #region Drawing
        #region Ellipses (and implicitly converted circles)
        /// <summary>
        /// Fills a given <see cref="Ellipse"/> using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be filled.</param>
        public void Fill(Ellipse ellipse)
        {
            CommitFillColor();
            GL.Begin(BeginMode.Polygon);
            DrawVertices(ellipse);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a given <see cref="Ellipse"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="ellipse">An <see cref="Ellipse"/> to be strokes.</param>
        public void Stroke(Ellipse ellipse)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.LineLoop);
            DrawVertices(ellipse);
            GL.End();
        }

        private void DrawVertices(Ellipse ellipse)
        {
            if (!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");

            for (int i = 0; i < unitCirclePoints.Length; ++i)
            {
                Vector2 unitCirclePoint = unitCirclePoints[i];
                float x = ellipse.Center.X + unitCirclePoint.X * ellipse.Radii.X;
                float y = ellipse.Center.Y + unitCirclePoint.Y * ellipse.Radii.Y;
                GL.Vertex2(x, y);
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
            GL.Begin(BeginMode.Quads);
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
            if (!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");

            GL.Vertex2(rectangle.MinX, rectangle.MinY);
            GL.Vertex2(rectangle.MinX, rectangle.MaxY);
            GL.Vertex2(rectangle.MaxX, rectangle.MaxY);
            GL.Vertex2(rectangle.MaxX, rectangle.MinY);
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
            if (!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");

            GL.Vertex2(triangle.Vertex1);
            GL.Vertex2(triangle.Vertex2);
            GL.Vertex2(triangle.Vertex3);
        }
        #endregion

        #region Lines
        /// <summary>
        /// Strokes a line using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="points">A sequence of points forming a line.</param>
        public void StrokeLineStrip(IEnumerable<Vector2> points)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.LineStrip);
            DrawVertices(points);
            GL.End();
        }

        public void StrokeLineStrip(params Vector2[] points)
        {
            StrokeLineStrip((IEnumerable<Vector2>)points);
        }

        private void DrawVertices(IEnumerable<Vector2> points)
        {
            if (!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");

            foreach (Vector2 point in points)
                GL.Vertex2(point);
        }

        /// <summary>
        /// Strokes the outline of a polygon using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="path">A <see href="LinePath"/> to stroke.</param>
        /// <param name="position">A position by which to offset the path's points.</param>
        public void Stroke(LinePath path, Vector2 position)
        {
            CommitStrokeColor();
            GL.Begin(BeginMode.Lines);
            DrawVertices(path.LineSegments, position);
            GL.End();
        }

        private void DrawVertices(IEnumerable<LineSegment> lineSegments, Vector2 position)
        {
            if (!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");

            foreach (LineSegment lineSegment in lineSegments)
            {
                GL.Vertex2(lineSegment.EndPoint1 + position);
                GL.Vertex2(lineSegment.EndPoint2 + position);
            }
        }
        #endregion

        #region Text

        /// <summary>
        /// Printes text, using this context's defined font and color, to the view at the origin coordinates
        /// </summary>
        /// <param name="text">The text to print</param>
        public void Draw(string text)
        {
            Draw(text, new Vector2(0, 0));
        }

        /// <summary>
        /// Prints text, using this context's defined font and color, to the view at specified coordinates. 
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to print</param>
        /// <param name="position">The position at which to print the string</param>
        public void Draw(string text, Vector2 position)
        {
            Draw(new Text(text), position);
        }

        public void Draw(Text text)
        {
            Draw(text, text.Frame);
        }

        public void Draw(Text text, Vector2 position)
        {
            Draw(text, text.Frame.TranslatedBy(position));
        }

        public void Draw(Text text, Rectangle drawInto)
        {
            GL.PushMatrix();
            GL.Translate(drawInto.MinX, drawInto.MinY, 0);
            GL.Scale(1, -1, 1);
            RectangleF renderInto = new RectangleF(0, -drawInto.Height, drawInto.Width, drawInto.Height);
            Text.defaultTextPrinter.Print(text.Value, text.Font, fillColor, renderInto);
            GL.PopMatrix();
        }

        #endregion

        #region Textured
        public void FillTextured(Rectangle rectangle, Texture texture)
        {
            Argument.EnsureNotNull(texture, "texture");

            GL.Color3(1f, 1f, 1f);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            if (texture.HasAlphaChannel)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex2(rectangle.MinX, rectangle.MinY);
            GL.TexCoord2(0, 1);
            GL.Vertex2(rectangle.MinX, rectangle.MaxY);
            GL.TexCoord2(1, 1);
            GL.Vertex2(rectangle.MaxX, rectangle.MaxY);
            GL.TexCoord2(1, 0);
            GL.Vertex2(rectangle.MaxX, rectangle.MinY);
            GL.End();
            if (texture.HasAlphaChannel) GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }
        #endregion
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
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Vector2[] unitCirclePoints;
        #endregion

        #region Constructor
        static GraphicsContext()
        {
            unitCirclePoints = new Vector2[32];
            double angleIncrement = Math.PI * 2 / unitCirclePoints.Length;
            for (int i = 0; i < unitCirclePoints.Length; ++i)
            {
                double angle = angleIncrement * i;
                double x = Math.Cos(angle);
                double y = Math.Sin(angle);
                unitCirclePoints[i] = new Vector2((float)x, (float)y);
            }
        }
        #endregion
        #endregion
    }
}
