using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Describes the type of a mouse event.
    /// </summary>
    public enum MouseEventType
    {
        /// <summary>
        /// Specifies that the event is a mouse move.
        /// </summary>
        Move,

        /// <summary>
        /// Specifies that the event is a button press or release.
        /// </summary>
        Button,

        /// <summary>
        /// Specifies that the event is a mouse wheel move.
        /// </summary>
        Wheel
    }
}
