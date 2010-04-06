using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Specifies the mode of a window between fullscreen and windowed.
    /// </summary>
    [Serializable]
    public enum WindowMode
    {
        /// <summary>
        /// Specifies that the window is a normal bordered OS window.
        /// </summary>
        Windowed,

        /// <summary>
        /// Specifies that the window takes the full area of the screen.
        /// </summary>
        Fullscreen
    }
}
