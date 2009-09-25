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
    /// <summary>
    /// A GraphicsContext object represents a space in which it's possible to draw. Methods to fill and stroke shapes are supplied.
    /// </summary>
    public partial class GraphicsContext
    {
        /// <summary>
        /// The bounds of the local coordinates parentSystem. 
        /// </summary>
        public Rectangle CoordsSystem { get; set; }
		
		/// <summary>
		/// The 'Color' member sets and gets the current drawing color.
		/// </summary>
		public System.Drawing.Color Color
		{
			get
			{
				float[] components = new float[4];
				GL.GetFloat(GetPName.CurrentColor, components);
                byte[] clampedComponents = components.Select(i => (byte)(i * 0xFF)).ToArray();
                return System.Drawing.Color.FromArgb(clampedComponents[3], clampedComponents[0], clampedComponents[1], clampedComponents[2]);
			}
			set { GL.Color4(value); }
		}

        /// <summary>
        /// Constructs a GraphicsContext object with given bounds for its local coordinates parentSystem. 
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="Rectangle"/> defining the local coordinates parentSystem
        /// </param>
        internal GraphicsContext(Rectangle bounds)
        {
            CoordsSystem = bounds;
        }

        internal void SetUpGLContext(Rectangle parentSystem)
        {
            GL.PushMatrix();

            // I'm not quite sure about the transformation order.
            // If something weird happens in the bounds scaling/translating, check this first.
            GL.Translate(parentSystem.Origin.X, parentSystem.Origin.Y, 0);
            GL.Scale(parentSystem.Size.X / CoordsSystem.Size.X, parentSystem.Size.Y / CoordsSystem.Size.Y, 1);
            GL.Translate(-CoordsSystem.Origin.X, -CoordsSystem.Origin.Y, 0);
        }

        internal void RestoreGLContext()
        {
            GL.PopMatrix();
        }
    }
}
