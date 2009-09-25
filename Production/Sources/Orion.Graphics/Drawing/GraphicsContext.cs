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
        /// The bounds of the local coordinates system. 
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
        /// Constructs a GraphicsContext object with given bounds for its local coordinates system. 
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="Rectangle"/> defining the local coordinates system
        /// </param>
        internal GraphicsContext(Rectangle bounds)
        {
            CoordsSystem = bounds;
        }

        internal void SetUpGLContext(Rectangle system)
        {
            GL.PushMatrix();
			
            GL.Scale(system.Size.X / CoordsSystem.Size.X, system.Size.Y / CoordsSystem.Size.Y, 1);
            GL.Translate(system.Position.X, system.Position.Y, 0);
            GL.Translate(-CoordsSystem.Position.X, -CoordsSystem.Position.Y, 0);

        }

        internal void RestoreGLContext()
        {
            GL.PopMatrix();
        }
    }
}
