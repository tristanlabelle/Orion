using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    // This class part defines members relating to mouse, keyboard and character input handling.
    partial class Control
    {
        #region Fields
        private bool isMouseEventSink;
        private bool isKeyEventSink;
        private bool hasEnabledFlag = true;
        #endregion

        #region Events
        #region Mouse
        private HandleableEvent<Control, MouseEvent> mouseMovedEvent;
        private HandleableEvent<Control, MouseEvent> mouseButtonEvent;
        private HandleableEvent<Control, MouseEvent> mouseWheelEvent;

        /// <summary>
        /// Raised when the mouse moves over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseMoved
        {
            add { mouseMovedEvent.AddHandler(value); }
            remove { mouseMovedEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when a mouse button is pressed or released over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseButton
        {
            add { mouseButtonEvent.AddHandler(value); }
            remove { mouseButtonEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when the mouse wheel is moved over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseWheel
        {
            add { mouseWheelEvent.AddHandler(value); }
            remove { mouseWheelEvent.RemoveHandler(value); }
        }
        #endregion

        #region Keyboard & Characters
        private HandleableEvent<Control, KeyEvent> keyEvent;
        private HandleableEvent<Control, char> characterTypedEvent;

        /// <summary>
        /// Raised when a key event occurs while this control has the keyboard focus.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, KeyEvent, bool> KeyEvent
        {
            add { keyEvent.AddHandler(value); }
            remove { keyEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when a character is typed while this control has the keyboard focus.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, char, bool> CharacterTyped
        {
            add { characterTypedEvent.AddHandler(value); }
            remove { characterTypedEvent.RemoveHandler(value); }
        }
        #endregion
        #endregion

        #region Properties
        #region Under Mouse
        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> or one of its descendants are under the mouse cursor.
        /// </summary>
        public bool IsUnderMouse
        {
            get { return manager != null && HasDescendant(manager.ControlUnderMouse); }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> is directly under the mouse cursor.
        /// </summary>
        public bool IsDirectlyUnderMouse
        {
            get { return manager != null && manager.ControlUnderMouse == this; }
        }
        #endregion

        #region Event Sink Flags
        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> always indicates
        /// mouse events as being handled.
        /// </summary>
        public bool IsMouseEventSink
        {
            get { return isMouseEventSink; }
            set { isMouseEventSink = value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> always indicates
        /// keyboard events as being handled.
        /// </summary>
        public bool IsKeyEventSink
        {
            get { return isKeyEventSink; }
            set { isKeyEventSink = value; }
        }
        #endregion

        #region Enabled
        /// <summary>
        /// Accesses a value indicating if this <see cref="Control"/> has the enabled flag.
        /// If <c>false</c>, this <see cref="Control"/> and its descendants will be disabled.
        /// </summary>
        public bool HasEnabledFlag
        {
            get { return hasEnabledFlag; }
            set { hasEnabledFlag = value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> is enabled,
        /// taking into account this <see cref="Control"/> and its ancestors' enabled flag.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                Control ancestor = this;
                do
                {
                    if (!ancestor.HasEnabledFlag) return false;
                    ancestor = ancestor.parent;
                } while (ancestor != null);

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating if this control
        /// can handle events even when it is disabled.
        /// </summary>
        protected virtual bool HandlesEventsWhenDisabled
        {
            get { return false; }
        }
        #endregion
        #endregion

        #region Methods
        #region Propagation Plumbing
        protected internal bool HandleMouseEvent(MouseEvent @event)
        {
            Debug.Assert(Visibility == Visibility.Visible);

            if (!HandlesEventsWhenDisabled && !IsEnabled) return false;

            switch (@event.Type)
            {
                case MouseEventType.Move: return HandleMouseMoved(@event);
                case MouseEventType.Button: return HandleMouseButton(@event);
                case MouseEventType.Wheel: return HandleMouseWheel(@event);
                default: throw new InvalidEnumArgumentException("type", (int)@event.Type, typeof(MouseEventType));
            }
        }

        private bool HandleMouseMoved(MouseEvent @event)
        {
            return OnMouseMoved(@event)
                | mouseMovedEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        private bool HandleMouseButton(MouseEvent @event)
        {
            return OnMouseButton(@event)
                | mouseButtonEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        private bool HandleMouseWheel(MouseEvent @event)
        {
            return OnMouseWheel(@event)
                | mouseWheelEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        internal bool HandleKeyEvent(KeyEvent @event)
        {
            Debug.Assert(Visibility == Visibility.Visible);

            if (!HandlesEventsWhenDisabled && !IsEnabled) return false;

            return OnKeyEvent(@event)
                | keyEvent.Raise(this, @event)
                | IsKeyEventSink;
        }

        internal bool HandleCharacterTyped(char character)
        {
            Debug.Assert(Visibility == Visibility.Visible);

            if (!HandlesEventsWhenDisabled && !IsEnabled) return false;

            return OnCharacterTyped(character)
                | characterTypedEvent.Raise(this, character);
        }
        #endregion

        #region Overridables
        /// <summary>
        /// When overriden in a derived class, handles a mouse move event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseMoved(MouseEvent @event) { return false; }

        /// <summary>
        /// When overriden in a derived class, handles a mouse button event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseButton(MouseEvent @event) { return false; }

        /// <summary>
        /// When overriden in a derived class, handles a mouse wheel event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseWheel(MouseEvent @event) { return false; }

        /// <summary>
        /// Gives a chance to this <see cref="Control"/> to handle a key event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnKeyEvent(KeyEvent @event) { return false; }

        /// <summary>
        /// Gives a chance to this <see cref="Control"/>to handle a character event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnCharacterTyped(char character) { return false; }

        /// <summary>
        /// Invoked when the mouse cursor enters this <see cref="Control"/>.
        /// </summary>
        protected internal virtual void OnMouseEntered() { }

        /// <summary>
        /// Invoked when the mouse cursor exits this <see cref="Control"/>.
        /// </summary>
        protected internal virtual void OnMouseExited() { }
        #endregion
        #endregion
    }
}
