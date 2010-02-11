using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Specifies a type of access to a resource.
    /// </summary>
    [Serializable]
    public enum Access
    {
        /// <summary>
        /// Specifies no access to a resource.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies read access to a resource.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Specifies write access to a resource.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Specifies read and write access to a resource.
        /// </summary>
        ReadWrite = Read | Write
    }
}
