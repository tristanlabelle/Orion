using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Represents a space in which it is possible to draw. Methods to fill and stroke shapes are supplied.
    /// </summary>
    public partial class GraphicsContext
    {
        #region Fields
        private Rectangle coordinateSystem;
        private Color fillColor = Color.White;
        private Color strokeColor = Color.Black;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref=GraphicsContext""/> object with the given bounds for its local coordinate system. 
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

            // I'm not quite sure about the transformation order.
            // If something weird happens in the bounds scaling/translating, check this first.
            GL.Translate(parentSystem.Origin.X, parentSystem.Origin.Y, 0);
            GL.Scale(parentSystem.Size.X / CoordinateSystem.Size.X, parentSystem.Size.Y / CoordinateSystem.Size.Y, 1);
            GL.Translate(-CoordinateSystem.Origin.X, -CoordinateSystem.Origin.Y, 0);
        }

        internal void RestoreGLContext()
        {
            GL.PopMatrix();
        }
        #endregion
    }
}
