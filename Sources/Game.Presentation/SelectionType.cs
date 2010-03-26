using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Describes the type of contents of a selection.
    /// </summary>
    [Serializable]
    public enum SelectionType
    {
        /// <summary>
        /// Indicates that the selection is empty.
        /// </summary>
        Empty,

        /// <summary>
        /// Indicates that the selection contains a single resource node.
        /// </summary>
        ResourceNode,

        /// <summary>
        /// Indicates that the selection contains one or more units.
        /// </summary>
        Units
    }
}
