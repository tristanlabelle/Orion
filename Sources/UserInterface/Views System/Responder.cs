using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;

namespace Orion.UserInterface
{
    public abstract class Responder : ViewContainer
    {
        #region Fields
        internal Vector2? cursorPosition;
        #endregion

        #region Constructors
        public Responder()
            : base()
        { }

        public Responder(Collection<ViewContainer> childrenCollection)
            : base(childrenCollection)
        { }
        #endregion

        #region Properties
        public Vector2? MousePosition
        {
            get
            {
                EnsureNotDisposed();
                return cursorPosition;
            }
        }

        public bool IsMouseOver
        {
            get
            {
                EnsureNotDisposed();
                return cursorPosition.HasValue;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a mouse button is pressed while the cursor is over this view.
        /// </summary>
        public event Action<Responder, MouseEventArgs> MouseButtonPressed;

        /// <summary>
        /// Raised when a mouse button is released, even if the cursor is not over this view.
        /// </summary>
        public event Action<Responder, MouseEventArgs> MouseButtonReleased;

        public event Action<Responder, MouseEventArgs> MouseMoved;
        public event Action<Responder, MouseEventArgs> MouseWheelScrolled;
        public event Action<Responder, MouseEventArgs> MouseEntered;
        public event Action<Responder, MouseEventArgs> MouseExited;
        public event Action<Responder, MouseEventArgs> DoubleClick;
        public event Action<Responder, KeyboardEventArgs> KeyboardButtonPressed;
        public event Action<Responder, KeyboardEventArgs> KeyboardButtonReleased;
        public event Action<Responder, char> CharacterTyped;
        #endregion

        #region Methods
        #region Event Propagation
        /// <summary>Propagates a mouse event to the child views.</summary>
        /// <remarks>
        /// Events are propagated in a bottom-up order, but priority of execution is given in an up-bottom order (we will call this "event sinking").
        /// </remarks>
        /// <param name="type">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal virtual bool PropagateMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            EnsureNotDisposed();

            bool eventCanSink = true;
            foreach (Responder child in Children.Reverse())
            {
                if (!child.Frame.ContainsPoint(args.Position))
                {
                    if (child.IsMouseOver)
                    {
                        child.PropagateMouseEvent(MouseEventType.MouseExited, args);
                        child.cursorPosition = null;
                    }
                }
                else
                {
                    if (eventCanSink)
                    {
                        if (!child.IsMouseOver)
                        {
                            child.DispatchMouseEvent(MouseEventType.MouseEntered, args);
                        }

                        child.cursorPosition = args.Position;
                        eventCanSink &= child.PropagateMouseEvent(type, args);
                    }
                }
            }

            return eventCanSink ? DispatchMouseEvent(type, args) : false;
        }

        /// <summary>Propagates a keyboard event to the child views.</summary>
        /// <remarks>
        /// Events are propagated in a bottom-up order, but priority of execution is given in an up-bottom order (we will call this "event sinking").
        /// </remarks>
        /// <param name="type">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="KeyboardEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal virtual bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            EnsureNotDisposed();
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                bool keepPropagating = child.PropagateKeyboardEvent(type, args);
                if (keepPropagating)
                    child.DispatchKeyboardEvent(type, args);
                else return false;
            }

            return DispatchKeyboardEvent(type, args);
        }

        protected internal virtual bool PropagateCharacterTypedEvent(char character)
        {
            EnsureNotDisposed();
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                bool keepPropagating = child.PropagateCharacterTypedEvent(character);
                if (!keepPropagating) return false;
            }

            return DispatchCharacterPressedEvent(character);
        }

        /// <summary>Propagates an update event to the child views.</summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last call, in seconds.</param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal virtual void PropagateUpdateEvent(float timeDeltaInSeconds)
        {
            EnsureNotDisposed();

            foreach (Responder child in Enumerable.Reverse(Children))
                child.PropagateUpdateEvent(timeDeltaInSeconds);

            Update(timeDeltaInSeconds);
        }
        #endregion

        #region Event Dispatch
        protected internal bool DispatchMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            EnsureNotDisposed();
            switch (eventType)
            {
                case MouseEventType.MouseButtonPressed: return OnMouseButtonPressed(args);
                case MouseEventType.MouseMoved: return OnMouseMoved(args);
                case MouseEventType.MouseButtonReleased: return OnMouseButtonReleased(args);
                case MouseEventType.MouseEntered: return OnMouseEntered(args);
                case MouseEventType.MouseExited: return OnMouseExited(args);
                case MouseEventType.MouseWheelScrolled: return OnMouseWheelScrolled(args);
                case MouseEventType.DoubleClick: return OnDoubleClick(args);
            }
            throw new NotImplementedException(String.Format("Mouse event type {0} does not have a handler method", eventType));
        }

        protected internal bool DispatchCharacterPressedEvent(char character)
        {
            EnsureNotDisposed();
            return OnCharacterPressed(character);
        }

        protected internal bool DispatchKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            EnsureNotDisposed();
            switch (type)
            {
                case KeyboardEventType.KeyDown: return OnKeyboardButtonPressed(args);
                case KeyboardEventType.KeyUp: return OnKeyboardButtonReleased(args);
            }
            throw new NotImplementedException(String.Format("Keyboard event type {0} does not have a handler method", type));
        }
        #endregion

        #region Event Handling
        #region Mouse Events
        /// <summary>
        /// Calls the event handler for mouse button pressing.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseButtonPressed(MouseEventArgs args)
        {
            MouseButtonPressed.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse moving.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseMoved(MouseEventArgs args)
        {
            MouseMoved.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button releasing.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseButtonReleased(MouseEventArgs args)
        {
            MouseButtonReleased.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse wheel scrolling. 
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseWheelScrolled(MouseEventArgs args)
        {
            MouseWheelScrolled.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse enter.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseEntered(MouseEventArgs args)
        {
            MouseEntered.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse exit.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseExited(MouseEventArgs args)
        {
            MouseExited.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse Double click. 
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnDoubleClick(MouseEventArgs args)
        {
            DoubleClick.Raise(this, args);
            return true;
        }
        #endregion

        #region Keyboard Events
        /// <summary>
        /// Calls the event handler for key pressing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnKeyboardButtonPressed(KeyboardEventArgs args)
        {
            KeyboardButtonPressed.Raise(this, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for key releasing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnKeyboardButtonReleased(KeyboardEventArgs args)
        {
            KeyboardButtonReleased.Raise(this, args);
            return true;
        }

        protected virtual bool OnCharacterPressed(char character)
        {
            CharacterTyped.Raise(this, character);
            return true;
        }
        #endregion

        #region Update Events
        protected virtual void Update(float timeDeltaInSeconds)
        {
            EnsureNotDisposed();
        }
        #endregion

        #region Hierarchy Events
        protected internal override void OnRemovedFromParent(ViewContainer parent)
        {
            if (cursorPosition.HasValue)
            {
                cursorPosition = null;
                PropagateMouseEvent(MouseEventType.MouseExited, new MouseEventArgs());
            }

            base.OnRemovedFromParent(parent);
        }
        #endregion
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MouseButtonPressed = null;
                MouseButtonReleased = null;
                MouseMoved = null;
                MouseWheelScrolled = null;
                MouseEntered = null;
                MouseExited = null;
                KeyboardButtonPressed = null;
                KeyboardButtonReleased = null;
                CharacterTyped = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
