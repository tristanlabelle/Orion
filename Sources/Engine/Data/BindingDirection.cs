using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Data
{
    /// <summary>
    /// Specifies the direction of the binding between values.
    /// </summary>
    public enum BindingDirection
    {
        /// <summary>
        /// Indicates that the binding does not automatically update source or destination values.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the destination should be updated whenever the source changes.
        /// </summary>
        SourceToDestination,

        /// <summary>
        /// Indicates that the source should be updated whenever the destination changes.
        /// </summary>
        DestinationToSource,

        /// <summary>
        /// Indicates that the source and destination should be kept synchronized.
        /// </summary>
        TwoWay
    }
}
