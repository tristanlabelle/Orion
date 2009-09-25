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
        public Rect CoordsSystem { get; set; }
		
		/// <summary>
		/// The 'Color' member sets and gets the current drawing color.
		/// </summary>
		public System.Drawing.Color Color
		{
			get
			{
				int[] components = new int[4];
				GL.GetInteger(GetPName.CurrentColor, components);
				return System.Drawing.Color.FromArgb(components[3], components[0], components[1], components[2]);
			}
			set { GL.Color4(value); }
		}

        /// <summary>
        /// Constructs a GraphicsContext object with given bounds for its local coordinates system. 
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="Rect"/> defining the local coordinates system
        /// </param>
        internal GraphicsContext(Rect bounds)
        {
            CoordsSystem = bounds;
        }

        internal void SetUpGLContext(Rect system)
        {
            GL.PushMatrix();
			
            GL.Scale(system.Size.X / CoordsSystem.Size.X, system.Size.Y / CoordsSystem.Size.Y, 1);
            GL.Translate(system.Position.X, system.Position.Y, 0);
        }

        internal void RestoreGLContext()
        {
            GL.PopMatrix();
        }
    }
}
