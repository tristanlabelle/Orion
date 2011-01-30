using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Specifies how a <see cref="Control"/> occupies space within its parent.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Specifies that the <see cref="Control"/> should be stretched to fill the whole available size.
        /// </summary>
        Stretch,

        /// <summary>
        /// Specifies that the <see cref="Control"/> should be aligned on the negative side of the axis.
        /// </summary>
        Negative,

        /// <summary>
        /// Specifies that the <see cref="Control"/> should be centered within its parent.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that the <see cref="Control"/> should be aligned on the positive side of the axis.
        /// </summary>
        Positive
    }
}
