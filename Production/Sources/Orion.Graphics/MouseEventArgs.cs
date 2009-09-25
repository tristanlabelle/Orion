using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// Indicates a mouse event type. Internal use only.
    /// </summary>
    internal enum MouseEventType
    {
        MouseDown, MouseUp, MouseClicked, MouseMoved
    }

    /// <summary>
    /// Flags for mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// No button pressed
        /// </summary>
        None = 0,

        /// <summary>
        /// Left mouse button
        /// </summary>
        Left = 1,

        /// <summary>
        /// Right mouse button
        /// </summary>
        Right = 2,

        /// <summary>
        /// Middle mouse button
        /// </summary>
        Middle = 4
    }

    /// <summary>
    /// This immutable structure encapsulates all necessary informations about a mouse click event:
    /// click position, which buttons were pressed and how many times they were.
    /// </summary>
    public struct MouseEventArgs
    {
        /// <summary>
        /// The abscissa of the mouse click in window coordinates
        /// </summary>
        public readonly float X;
        /// <summary>
        /// The ordinate of the mouse click in window coordinates
        /// </summary>
        public readonly float Y;
        /// <summary>
        /// Which buttons were pressed (normally only one per event)
        /// </summary>
        public readonly MouseButton ButtonPressed;
        /// <summary>
        /// How many consecutive clicks were there (if any)
        /// </summary>
        public readonly int Clicks;
        /// <summary>
        /// A shorthand property to get a vector containing the position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
        }

        /// <summary>
        /// Creates a MouseEventArgs using the data contained in a <see cref="System.Windows.Forms.MouseEventArgs"/> object
        /// to pass it around to <see cref="View"/> subclasses event listeners.
        /// </summary>
        /// <param name="args">The <see cref="System.Windows.Forms.MouseEventArgs"/> originating event</param>
        internal MouseEventArgs(System.Windows.Forms.MouseEventArgs args)
        {
            X = args.X;
            Y = args.Y;

            ButtonPressed = MouseButton.None;
            switch (args.Button)
            {
                case System.Windows.Forms.MouseButtons.Left: ButtonPressed = MouseButton.Left; break;
                case System.Windows.Forms.MouseButtons.Middle: ButtonPressed = MouseButton.Middle; break;
                case System.Windows.Forms.MouseButtons.Right: ButtonPressed = MouseButton.Right; break;
            }
            Clicks = args.Clicks;
        }
    }
}
