using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

namespace Orion
{
    /// <summary>
    /// Represents possible types of keyboard events.
    /// </summary>
    public enum KeyboardEventType
    {
        /// <summary>
        /// The type for events for which the user pressed a key.
        /// </summary>
        KeyDown,

        /// <summary>
        /// The type for events for which the user released a key.
        /// </summary>
        KeyUp
    }

    /// <summary>
    /// This immutable structure encapsulates data about a keyboard event.
    /// </summary>
    public struct KeyboardEventArgs
    {
        /// <summary>
        /// Indicates if the Alt key was pressed when the event was generated.
        /// </summary>
        public readonly bool HasAlt;

        /// <summary>
        /// Indicates if the Control key was pressed when the event was generated.
        /// </summary>
        public readonly bool HasControl;

        /// <summary>
        /// Indicates if the Shift key was pressed when the event was generated.
        /// </summary>
        public readonly bool HasShift;

        /// <summary>
        /// Indicates which virtual key code was pressed.
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// Creates a new KeyboardEventArgs structure.
        /// </summary>
        /// <param name="key">The pressed virtual key</param>
        /// <param name="alt">Whether the alt key was pressed</param>
        /// <param name="control">Whether the control key was pressed</param>
        /// <param name="shift">Whether the shift key was pressed</param>
        public KeyboardEventArgs(Keys key, bool alt, bool control, bool shift)
        {
            HasAlt = alt;
            HasControl = control;
            HasShift = shift;
            Key = key;
        }
    }
}
