using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Indicates a type of mouse event. Internal use only.
    /// </summary>
    [Serializable]
    [Obsolete("To be made internal.")]
    public enum MouseEventType
    {
        None,
        MouseButtonPressed,
        MouseButtonReleased,
        MouseMoved,
        MouseEntered,
        MouseExited,
        MouseWheelScrolled,
        DoubleClick
    }
}
