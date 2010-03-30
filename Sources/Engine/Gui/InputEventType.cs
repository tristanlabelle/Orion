using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Describes the type of a window event that was received.
    /// </summary>
    [Serializable]
    public enum InputEventType
    {
        Keyboard,
        Mouse,
        Character
    }
}
