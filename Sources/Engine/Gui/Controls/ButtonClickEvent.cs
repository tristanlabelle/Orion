using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Describes the event of a <see cref="Button"/> getting clicked.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct ButtonClickEvent
    {
        #region Fields
        /// <summary>
        /// A <see cref="ButtonClickEvent"/> with a <see cref="ButtonClickType.Programmatic"/> type.
        /// </summary>
        public static readonly ButtonClickEvent Programmatic
            = new ButtonClickEvent(ButtonClickType.Programmatic, default(MouseEvent), default(KeyEvent));

        private readonly ButtonClickType type;
        private readonly MouseEvent mouseEvent;
        private readonly KeyEvent keyEvent;
        #endregion

        #region Constructors
        private ButtonClickEvent(ButtonClickType type, MouseEvent mouseEvent, KeyEvent keyEvent)
        {
            this.type = type;
            this.mouseEvent = mouseEvent;
            this.keyEvent = keyEvent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the source of this event.
        /// </summary>
        public ButtonClickType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the <see cref="MouseEvent"/> which caused the <see cref="Button"/> to get clicked.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Type"/> is not <see cref="ButtonClickType.Mouse"/>.
        /// </exception>
        public MouseEvent MouseEvent
        {
            get
            {
                if (type != ButtonClickType.Mouse) throw new InvalidOperationException();
                return mouseEvent;
            }
        }

        /// <summary>
        /// Gets the <see cref="KeyEvent"/> which caused the <see cref="Button"/> to get clicked.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Type"/> is not <see cref="ButtonClickType.Keyboard"/>.
        /// </exception>
        public KeyEvent KeyEvent
        {
            get
            {
                if (type != ButtonClickType.Keyboard) throw new InvalidOperationException();
                return keyEvent;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new event with a <see cref="ButtonClickType.Mouse"/> type.
        /// </summary>
        /// <param name="mouseEvent">The mouse event that caused the click.</param>
        /// <returns>A new mouse-based <see cref="ButtonClickEvent"/>.</returns>
        public static ButtonClickEvent CreateMouse(MouseEvent mouseEvent)
        {
            return new ButtonClickEvent(ButtonClickType.Mouse, mouseEvent, default(KeyEvent));
        }

        /// <summary>
        /// Creates a new event with a <see cref="ButtonClickType.Keyboard"/> type.
        /// </summary>
        /// <param name="keyEvent">The keyboard event that caused the click.</param>
        /// <returns>A new keyboard-based <see cref="ButtonClickEvent"/>.</returns>
        public static ButtonClickEvent CreateMouse(KeyEvent keyEvent)
        {
            return new ButtonClickEvent(ButtonClickType.Keyboard, default(MouseEvent), keyEvent);
        }

        public override string ToString()
        {
            return type.ToStringInvariant();
        }
        #endregion
    }
}
