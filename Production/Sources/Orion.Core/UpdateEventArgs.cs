using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Objects of this structure encapsulate the data of a frame update.
    /// </summary>
    public struct UpdateEventArgs
    {
        /// <summary>
        /// The time, in seconds, since the last Update event was triggered
        /// </summary>
        public readonly float Delta;

        /// <summary>
        /// Constructs a new Update event arguments structure with a given time delta.
        /// </summary>
        /// <param name="delta">The time in seconds since the last Update event was triggered.</param>
        public UpdateEventArgs(float delta)
        {
            Delta = delta;
        }
    }
}
