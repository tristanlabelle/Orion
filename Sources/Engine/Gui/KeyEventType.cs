using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Describes a type of keyboard event.
    /// </summary>
    public enum KeyEventType
    {
        /// <summary>
        /// Specifies that a key was pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// Specifies that a key was released.
        /// </summary>
        Released,

        /// <summary>
        /// Specifies that a key was held down, causing it to be logically pressed again.
        /// </summary>
        Repeated
    }
}
