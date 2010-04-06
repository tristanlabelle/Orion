using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Input
{
    /// <summary>
    /// Identifies a button of the mouse.
    /// </summary>
    [Serializable]
    public enum MouseButton
    {
        /// <summary>
        /// No mouse button.
        /// </summary>
        None,

        /// <summary>
        /// The left button of the mouse.
        /// </summary>
        Left,

        /// <summary>
        /// The middle button of the mouse.
        /// </summary>
        Middle,

        /// <summary>
        /// The right button of the mouse.
        /// </summary>
        Right
    }
}
