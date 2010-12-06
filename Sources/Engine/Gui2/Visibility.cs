using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Specifies the visibility of a <see cref="UIElement"/>.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Specifies that the element is fully visible and occupies physical space.
        /// </summary>
        Visible,

        /// <summary>
        /// Specifies that the element is not visible and does not occupy physical space.
        /// </summary>
        Collapsed,

        /// <summary>
        /// Specifies that the element is not visible but still occupies physical space.
        /// </summary>
        Hidden
    }
}
