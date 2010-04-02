using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Input
{
    /// <summary>
    /// Represents possible types of keyboard events.
    /// </summary>
    [Serializable]
    public enum KeyboardEventType
    {
        /// <summary>
        /// The type for events for which the user pressed a key.
        /// </summary>
        ButtonPressed,

        /// <summary>
        /// The type for events for which the user released a key.
        /// </summary>
        ButtonReleased
    }
}
