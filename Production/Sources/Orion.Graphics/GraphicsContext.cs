﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
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
            GL.Begin(BeginMode.Polygon);
            CommitFillColor();
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
            GL.Begin(BeginMode.LineLoop);
            CommitStrokeColor();
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
                GL.Vertex2(ellipse.Center.X + ellipse.Radii.X * Math.Cos(angle),
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
            GL.Begin(BeginMode.Polygon);
            CommitFillColor();
            DrawVertices(rectangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Rectangle"/> using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="rectangle">A <see href="Rectangle"/> to stroke.</param>
        public void Stroke(Rectangle rectangle)
        {
            GL.Begin(BeginMode.LineLoop);
            CommitStrokeColor();
            DrawVertices(rectangle);
            GL.End();
        }

        private void DrawVertices(Rectangle rectangle)
        {
			if(!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");
            GL.Vertex2(rectangle.X, rectangle.Y);
            GL.Vertex2(rectangle.X, rectangle.MaxY);
            GL.Vertex2(rectangle.MaxX, rectangle.MaxY);
            GL.Vertex2(rectangle.MaxX, rectangle.Y);
        }
        #endregion

        #region Triangles
        /// <summary>
        /// Fills a <see cref="Triangle"/> shape using the current <see cref="P:FillColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be filled.</param>
        public void Fill(Triangle triangle)
        {
            GL.Begin(BeginMode.Triangles);
            CommitFillColor();
            DrawVertices(triangle);
            GL.End();
        }

        /// <summary>
        /// Strokes the outline of a <see cref="Triangle"/> shape using the current <see cref="P:StrokeColor"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle"/> to be stroked.</param>
        public void Stroke(Triangle triangle)
        {
            GL.Begin(BeginMode.LineLoop);
            CommitStrokeColor();
            DrawVertices(triangle);
            GL.End();
        }

        private void DrawVertices(Triangle triangle)
        {
			if(!readyForDrawing) throw new InvalidOperationException("Cannot draw in an unprepared graphics context");
            GL.Vertex2(triangle.Vertex1);
            GL.Vertex2(triangle.Vertex2);
            GL.Vertex2(triangle.Vertex3);
        }
        #endregion

        #region Text
        /*TextPrinter printer = new TextPrinter();
        Font sans_serif = new Font(FontFamily.GenericSansSerif, 18.0f);

        public void DrawTextInView(string text, View view)
        {
            printer.Begin();
            GL.Translate(view.Frame.Origin.X, view.Frame.Origin.Y, 0);
            printer.Print(text, sans_serif, Color.Black);
            printer.End();
        }*/

        // ----------------------------------------------------------------------------------

        /*

        // Crée un TextPrinter à chaque opération de dessin.
        // Utilise la property 'Font' comme font à utiliser dans la méthode Print.
        public Font Font { get; set; }
        // Pour la couleur, utilise la property 'Color' définie quelque part dans GraphicsContext.cs.

        public void FillText(string text) { } // FillText(0,0, text)
        public void FillText(Vector2 position, string text) { } // FillText(position.X, position.Y, text);
        public void FillText(float x, float y, string text) { } // implémentation complète ici

        // supprime les commentaires d'instructions, et fais des tags de documentation
        //  (si possible à la mode de ceux des autres méthodes de la classe).

        // merci :)
         * 
         * */
        #endregion
        #endregion

        #region Non-Public
        /// <summary>
        /// Commits any changes to <see cref="FillColor"/> to OpenGL.
        /// </summary>
        private void CommitFillColor()
        {
            GL.Color4(fillColor.R, fillColor.G, fillColor.B, fillColor.A);
        }

        /// <summary>
        /// Commits any changes to <see cref="StrokeColor"/> to OpenGL.
        /// </summary>
        private void CommitStrokeColor()
        {
            GL.Color4(strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A);
        }

        internal void SetUpGLContext(Rectangle parentSystem)
        {
            GL.PushMatrix();

            // I'm not quite sure about the transformation order. This looks functional, though.
            // If something weird happens in the bounds scaling/translating, check this first.
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
