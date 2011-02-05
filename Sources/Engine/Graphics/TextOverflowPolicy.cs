using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Describes how cases where the text overflows its constraining bounds should be handled.
    /// </summary>
    [Serializable]
    public enum TextOverflowPolicy
    {
        /// <summary>
        /// Specifies that the last word should be ellipsisized.
        /// </summary>
        Ellipsis,

        /// <summary>
        /// Specifies that the text should wrap to the next line.
        /// </summary>
        Wrap,

        /// <summary>
        /// Specifies that the text should be clipped.
        /// </summary>
        Clip
    }
}
