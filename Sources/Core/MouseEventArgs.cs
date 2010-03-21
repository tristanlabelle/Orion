using System;
using System.ComponentModel;
using OpenTK.Math;

namespace Orion
{
    /// <summary>
    /// Indicates a mouse event type. Internal use only.
    /// </summary>
    [Serializable]
    public enum MouseEventType
    {
        None,
        MouseButtonPressed,
        MouseButtonReleased,
        MouseMoved,
        MouseEntered,
        MouseExited,
        MouseWheelScrolled,
        DoubleClick
    }

    /// <summary>
    /// Flags for mouse buttons.
    /// </summary>
    [Serializable]
    public enum MouseButton
    {
        /// <summary>
        /// No button pressed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Left mouse button.
        /// </summary>
        Left,

        /// <summary>
        /// Right mouse button.
        /// </summary>
        Right,

        /// <summary>
        /// Middle mouse button.
        /// </summary>
        Middle
    }

    /// <summary>
    /// This immutable structure encapsulates all necessary informations about a mouse click event:
    /// click position, which buttons were pressed and how many times they were.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct MouseEventArgs
    {
        #region Fields
        /// <summary>
        /// The abscissa of the mouse click in window coordinates.
        /// </summary>
        public readonly float X;

        /// <summary>
        /// The ordinate of the mouse click in window coordinates.
        /// </summary>
        public readonly float Y;

        /// <summary>
        /// For mouse button pressed or released events, which button is concerned.
        /// </summary>
        public readonly MouseButton Button;

        /// <summary>
        /// For mouse button pressed events, the number of consecutive clicks.
        /// </summary>
        public readonly int ClickCount;

        /// <summary>
        /// For mouse wheel moved events, the amount of movement of the mouse wheel,
        /// where 1 corresponds to a typical wheel notch.
        /// </summary>
        public readonly float WheelDelta;
        #endregion

        #region Constructors
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
        /// <param name="clickCount">
        /// The number of repeated clicks with the <see cref="MouseButton"/>.
        /// </param>
        public MouseEventArgs(float x, float y, MouseButton button, int clickCount, float wheelDelta)
        {
            X = x;
            Y = y;
            Button = button;
            ClickCount = clickCount;
            WheelDelta = wheelDelta;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Convenience property that returns a <see cref="Vector2"/> from this object's X and Y fields.
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Mouse Event Args position={0}, button={1}, delta={2}"
                .FormatInvariant(Position, Button, WheelDelta);
        }
        #endregion
    }
}