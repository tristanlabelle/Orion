using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;

namespace Orion.Graphics.Drawing
{
    /// <summary>
    /// The IDrawable interface tells that an object has OpenGL drawing capabilites.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// The method invoked by a GraphicsContext to fill the <see cref="IDrawable"/> object.
        /// </summary>
        void Fill();

        /// <summary>
        /// The method invoked by a GraphicsContext to stroke the <see cref="IDrawable"/> object.
        /// </summary>
        void Stroke();
    }
}
