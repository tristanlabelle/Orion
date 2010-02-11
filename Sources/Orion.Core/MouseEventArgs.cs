
using OpenTK.Math;

namespace Orion
{
    /// <summary>
    /// Indicates a mouse event type. Internal use only.
    /// </summary>
    public enum MouseEventType
    {
        None, MouseDown, MouseUp, MouseMoved, MouseEntered, MouseExited, MouseWheel, DoubleClick
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
        /// How many notches the mouse wheel has rotated. 
        /// </summary>
        public readonly int WheelDelta;

        /// <summary>
        /// Convenience property that returns a <see cref="Vector2"/> from this object's X and Y fields.
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
        }

        /// <summary>
        /// Creates a MouseEventArgs structure with a specified point, which mouse button was pressed, and the number of repeated clicks. 
        /// The X and Y positions should be directly usable for views.
        /// </summary>
        /// <param name="x">
        /// The position along the X axis of the mouse
        /// </param>
        /// <param name="y">
        /// The position along the Y axis of the mouse
        /// </param>
        /// <param name="button">
        /// The <see cref="MouseButton"/> involved
        /// </param>
        /// <param name="clicks">
        /// The number of repeated clicks with the <see cref="MouseButton"/>
        /// </param>
        public MouseEventArgs(float x, float y, MouseButton button, int clicks, int delta)
        {
            X = x;
            Y = y;
            ButtonPressed = button;
            Clicks = clicks;
            WheelDelta = delta;
        }

        public override string ToString()
        {
            return "Mouse Event Args position={0}, button={1}, delta={2}".FormatInvariant(Position, ButtonPressed, WheelDelta);
        }
    }
}