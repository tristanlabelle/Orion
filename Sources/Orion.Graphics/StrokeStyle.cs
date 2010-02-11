using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
