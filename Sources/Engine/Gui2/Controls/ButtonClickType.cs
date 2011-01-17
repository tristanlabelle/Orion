using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Specifies the type of a <see cref="Button.Clicked"/> event.
    /// </summary>
    public enum ButtonClickType
    {
        /// <summary>
        /// Indicates that the <see cref="Button"/> was clicked programatically.
        /// </summary>
        Programmatic,

        /// <summary>
        /// Indicates that the <see cref="Button"/> was clicked using the mouse.
        /// </summary>
        Mouse,

        /// <summary>
        /// Indicates that the <see cref="Button"/> was clicked using the keyboard.
        /// </summary>
        Keyboard
    }
}
