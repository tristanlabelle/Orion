using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Specifies the state of keyboard modifier keys.
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// Specifies that no modifier key is down.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the shift modifier key is down.
        /// </summary>
        Shift = 1 << 0,

        /// <summary>
        /// Specifies that the control modifier key is down.
        /// </summary>
        Control = 1 << 1,

        /// <summary>
        /// Specifies that the alt modifier key is down.
        /// </summary>
        Alt = 1 << 2
    }
}
