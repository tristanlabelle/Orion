using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Describes a mouse event.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct MouseEvent
    {
        #region Fields
        private readonly float wheelDelta;
        private readonly short x, y;
        private readonly byte buttonStates;
        private readonly byte modifierKeys;
        private readonly byte button;
        private readonly byte clickCount;
        #endregion

        #region Constructors
        public MouseEvent(Point position, MouseButtons buttonStates, ModifierKeys modifierKeys, MouseButtons button, float wheelDelta, int clickCount)
        {
            if (button != MouseButtons.None && !PowerOfTwo.Is((int)button))
                throw new ArgumentException("A single button should be specified.", "button");
            Argument.EnsurePositive(clickCount, "clickCount");

            this.x = checked((short)position.X);
            this.y = checked((short)position.Y);
            this.buttonStates = checked((byte)buttonStates);
            this.modifierKeys = checked((byte)modifierKeys);
            this.button = checked((byte)button);
            this.wheelDelta = wheelDelta;
            this.clickCount = checked((byte)clickCount);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of this mouse event.
        /// </summary>
        public MouseEventType Type
        {
            get
            {
                if (wheelDelta != 0) return MouseEventType.Wheel;
                if (Button == MouseButtons.None) return MouseEventType.Move;
                return clickCount == 0 ? MouseEventType.Button : MouseEventType.Click;
            }
        }

        /// <summary>
        /// Gets the position of the mouse when the event occured.
        /// </summary>
        public Point Position
        {
            get { return new Point(x, y); }
        }

        /// <summary>
        /// Gets the X coordinate of the mouse when the event occured.
        /// </summary>
        public int X
        {
            get { return x; }
        }

        /// <summary>
        /// Gets the Y coordinate of the mouse when the event occured.
        /// </summary>
        public int Y
        {
            get { return y; }
        }

        /// <summary>
        /// Gets the state of the mouse buttons when the event occured.
        /// </summary>
        public MouseButtons ButtonStates
        {
            get { return (MouseButtons)buttonStates; }
        }

        /// <summary>
        /// Gets a value indicating if the left mouse button was down when the event occured.
        /// </summary>
        public bool IsLeftButtonDown
        {
            get { return (ButtonStates & MouseButtons.Left) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the right mouse button was down when the event occured.
        /// </summary>
        public bool IsRightButtonDown
        {
            get { return (ButtonStates & MouseButtons.Right) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the middle mouse button was down when the event occured.
        /// </summary>
        public bool IsMiddleButtonDown
        {
            get { return (ButtonStates & MouseButtons.Middle) != 0; }
        }

        /// <summary>
        /// Gets the modifier keys that were pressed when the event occured.
        /// </summary>
        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)modifierKeys; }
        }

        /// <summary>
        /// Gets a value indicating if the shift modifier key was down when the event occured.
        /// </summary>
        public bool IsShiftDown
        {
            get { return (ModifierKeys & ModifierKeys.Shift) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the control modifier key was down when the event occured.
        /// </summary>
        public bool IsControlDown
        {
            get { return (ModifierKeys & ModifierKeys.Control) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the alt modifier key was down when the event occured.
        /// </summary>
        public bool IsAltDown
        {
            get { return (ModifierKeys & ModifierKeys.Alt) != 0; }
        }

        /// <summary>
        /// Gets the button that was pressed or released, for such events.
        /// </summary>
        public MouseButtons Button
        {
            get { return (MouseButtons)button; }
        }

        /// <summary>
        /// Gets a value indicating if the button was pressed, for button events.
        /// </summary>
        public bool IsPressed
        {
            get { return (ButtonStates & Button) != 0; }
        }

        /// <summary>
        /// Gets a value indicating if the button was released, for button events.
        /// </summary>
        public bool IsReleased
        {
            get { return (ButtonStates & Button) == 0; }
        }

        /// <summary>
        /// Gets the number of successive clicks, for click events.
        /// </summary>
        public int ClickCount
        {
            get { return clickCount; }
        }

        /// <summary>
        /// Gets the movement of the mouse wheel in notches, for mouse wheel events.
        /// </summary>
        public float WheelDelta
        {
            get { return wheelDelta; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            switch (Type)
            {
                case MouseEventType.Move: return "Moved to " + Position;
                case MouseEventType.Button: return "{0} button {1} at {2}".FormatInvariant(Button, IsPressed ? "pressed" : "released", Position);
                case MouseEventType.Wheel: return "Wheel rolled by {0} at {1}".FormatInvariant(WheelDelta, Position);
                case MouseEventType.Click: return "Click #{0} at {1}".FormatInvariant(ClickCount, Position);
                default:
                    Debug.Fail("Unexpected mouse event type.");
                    return base.ToString();
            }
        }

        public static MouseEvent CreateMove(Point position, MouseButtons buttonStates, ModifierKeys modifierKeys)
        {
            return new MouseEvent(position, buttonStates, modifierKeys, MouseButtons.None, 0f, 0);
        }

        public static MouseEvent CreateButton(Point position, MouseButtons buttonStates, ModifierKeys modifierKeys, MouseButtons button, bool pressed)
        {
            Argument.EnsureNotEqual(button, MouseButtons.None, "button");

            buttonStates &= ~button;
            if (pressed) buttonStates |= button;

            return new MouseEvent(position, buttonStates, modifierKeys, button, 0f, 0);
        }

        public static MouseEvent CreateClick(Point position, MouseButtons buttonStates, ModifierKeys modifierKeys, MouseButtons button, int count)
        {
            Argument.EnsureNotEqual(button, MouseButtons.None, "button");
            Argument.EnsureStrictlyPositive(count, "count");
            return new MouseEvent(position, buttonStates, modifierKeys, button, 0f, count);
        }

        public static MouseEvent CreateWheel(Point position, MouseButtons buttonStates, ModifierKeys modifierKeys, float wheelDelta)
        {
            Argument.EnsureFinite(wheelDelta, "wheelDelta");
            Argument.EnsureNotEqual(wheelDelta, 0, "wheelDelta");

            return new MouseEvent(position, buttonStates, modifierKeys, MouseButtons.None, wheelDelta, 0);
        }
        #endregion
    }
}
