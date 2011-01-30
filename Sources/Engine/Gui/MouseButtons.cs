using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Specifies the state of mouse buttons.
    /// </summary>
    [Flags]
    public enum MouseButtons
    {
        /// <summary>
        /// Specifies that no mouse button is down.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the left mouse button is down.
        /// </summary>
        Left = 1 << 0,

        /// <summary>
        /// Specifies that the right mouse button is down.
        /// </summary>
        Right = 1 << 1,

        /// <summary>
        /// Specifies that the middle mouse button is down.
        /// </summary>
        Middle = 1 << 2,
    }
}
