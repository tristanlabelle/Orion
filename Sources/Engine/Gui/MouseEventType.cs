using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Indicates a type of GUI mouse event.
    /// </summary>
    [Serializable]
    public enum MouseEventType
    {
        None,
        ButtonPressed,
        ButtonReleased,
        Moved,
        Entered,
        Exited,
        WheelScrolled
    }
}
