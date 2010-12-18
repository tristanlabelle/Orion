using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Describes the way a child <see cref="UIElement"/> is arranged within a <see cref="DockPanel"/>.
    /// </summary>
    public enum Dock
    {
        /// <summary>
        /// Specifies that the <see cref="UIElement"/> should be aligned on the negative X axis side.
        /// </summary>
        MinX,

        /// <summary>
        /// Specifies that the <see cref="UIElement"/> should be aligned on the negative Y axis side.
        /// </summary>
        MinY,

        /// <summary>
        /// Specifies that the <see cref="UIElement"/> should be aligned on the positive X axis side.
        /// </summary>
        MaxX,

        /// <summary>
        /// Specifies that the <see cref="UIElement"/> should be aligned on the positive Y axis side.
        /// </summary>
        MaxY
    }
}
