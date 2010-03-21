using System;
using System.ComponentModel;
using System.Diagnostics;
using OpenTK.Math;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// This immutable structure encapsulates all necessary informations about a mouse click event:
    /// click position, which buttons were pressed and how many times they were.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [DebuggerDisplay("Position={Position}, Button={Button}")]
    public struct MouseEventArgs
    {
        #region Fields
        /// <summary>
        /// The position of the mouse click.
        /// </summary>
        public readonly Vector2 Position;

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
        /// <param name="position">
        /// The position along the of the mouse.
        /// </param>
        /// <param name="button">
        /// The <see cref="MouseButton"/> involved.
        /// </param>
        /// <param name="clickCount">
        /// The number of repeated clicks with the <see cref="MouseButton"/>.
        /// </param>
        public MouseEventArgs(Vector2 position, MouseButton button, int clickCount, float wheelDelta)
        {
            Position = position;
            Button = button;
            ClickCount = clickCount;
            WheelDelta = wheelDelta;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the X coordinate of the point that was clicked.
        /// </summary>
        public float X
        {
            get { return Position.X; }
        }

        /// <summary>
        /// Gets the Y coordinate of the point that was clicked.
        /// </summary>
        public float Y
        {
            get { return Position.Y; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clones this <see cref="MouseEventArgs"/> but changes the position property.
        /// </summary>
        /// <param name="newPosition">The position of the mouse in the cloned event args.</param>
        /// <returns>The new event args with the modified position.</returns>
        public MouseEventArgs CloneWithNewPosition(Vector2 newPosition)
        {
            return new MouseEventArgs(newPosition, Button, ClickCount, WheelDelta);
        }
        #endregion
    }
}