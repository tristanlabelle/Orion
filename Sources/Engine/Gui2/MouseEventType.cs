using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
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
        Wheel,

        /// <summary>
        /// Specifies that the event is a click (a mouse down followed by a mouse up within some time and space constraints).
        /// </summary>
        Click
    }
}
