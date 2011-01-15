using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Specifies the visibility of a <see cref="Control"/>.
    /// </summary>
    /// <remarks>
    /// The enumerant values are ordered from least to most visible.
    /// </remarks>
    public enum Visibility
    {
        /// <summary>
        /// Specifies that the <see cref="Control"/> is not visible and does not occupy physical space.
        /// </summary>
        Collapsed = -2,

        /// <summary>
        /// Specifies that the <see cref="Control"/> is not visible but still occupies physical space.
        /// </summary>
        Hidden = -1,

        /// <summary>
        /// Specifies that the <see cref="Control"/> is fully visible and occupies physical space.
        /// </summary>
        Visible = 0
    }
}
