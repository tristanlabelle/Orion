using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Records the state of the mouse, including its position and buttons.
    /// </summary>
    public struct MouseState
    {
        #region Fields
        /// <summary>
        /// The current position of the mouse cursor.
        /// </summary>
        public readonly Point Position;

        /// <summary>
        /// A flag enumerant indicating which buttons are down.
        /// </summary>
        public readonly MouseButtons Buttons;
        #endregion

        #region Constructors
        public MouseState(Point position, MouseButtons buttons)
        {
            this.Position = position;
            this.Buttons = buttons;
        }

        public MouseState(int x, int y, MouseButtons buttons)
        {
            this.Position = new Point(x, y);
            this.Buttons = buttons;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the X coordinate of the mouse cursor.
        /// </summary>
        public int X
        {
            get { return Position.X; }
        }

        /// <summary>
        /// Gets the Y coordinate of the mouse cursor.
        /// </summary>
        public int Y
        {
            get { return Position.Y; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if one or more buttons are currently down.
        /// </summary>
        /// <param name="button">The button or buttons to be tested.</param>
        /// <returns><c>True</c> if <paramref name="button"/> is down, <c>false</c> otherwise.</returns>
        public bool IsDown(MouseButtons button)
        {
            return (Buttons & button) == button;
        }

        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(MouseState other)
        {
            return Position == other.Position && Buttons == other.Buttons;
        }

        public override bool Equals(object obj)
        {
            return obj is MouseState && Equals((MouseState)obj);
        }

        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(MouseState a, MouseState b)
        {
            return a.Equals(b);
        }

        public override string ToString()
        {
            return "{0} {1}".FormatInvariant(Position, Buttons);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(MouseState a, MouseState b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(MouseState a, MouseState b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}
