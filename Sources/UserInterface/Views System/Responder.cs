using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;

namespace Orion.UserInterface
{
    public abstract class Responder : ViewContainer
    {
        #region Fields
        private Vector2? cursorPosition;
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
            set
            {
                EnsureNotDisposed();
                cursorPosition = value;
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
        public event Action<Responder, MouseEventArgs> MouseDown;
        public event Action<Responder, MouseEventArgs> MouseUp;
        public event Action<Responder, MouseEventArgs> MouseMoved;
        public event Action<Responder, MouseEventArgs> MouseWheel;
        public event Action<Responder, MouseEventArgs> MouseEntered;
        public event Action<Responder, MouseEventArgs> MouseExited;
        public event Action<Responder, MouseEventArgs> DoubleClick;
        public event Action<Responder, KeyboardEventArgs> KeyDown;
        public event Action<Responder, KeyboardEventArgs> KeyUp;
        public event Action<Responder, char> KeyPress;
        public event Action<Responder, UpdateEventArgs> Updated;
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
                        child.MousePosition = null;
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
                        child.MousePosition = args.Position;
                        eventCanSink &= child.PropagateMouseEvent(type, args);
                    }
                }
            }

            if (eventCanSink)
            {
                return DispatchMouseEvent(type, args);
            }
            return false;
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

        protected internal virtual bool PropagateKeyPressEvent(char character)
        {
            EnsureNotDisposed();
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                bool keepPropagating = child.PropagateKeyPressEvent(character);
                if (!keepPropagating) return false;
            }

            return DispatchKeyPressEvent(character);
        }

        /// <summary>Propagates an update event to the child views.</summary>
        /// <param name="args">The <see cref="UpdateEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal virtual void PropagateUpdateEvent(UpdateEventArgs args)
        {
            EnsureNotDisposed();
            foreach (Responder child in Enumerable.Reverse(Children)) child.PropagateUpdateEvent(args);
            OnUpdate(args);
        }
        #endregion

        #region Event Dispatch
        protected internal bool DispatchMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            EnsureNotDisposed();
            switch (eventType)
            {
                case MouseEventType.MouseDown: return OnMouseDown(args);
                case MouseEventType.MouseMoved: return OnMouseMove(args);
                case MouseEventType.MouseUp: return OnMouseUp(args);
                case MouseEventType.MouseEntered: return OnMouseEnter(args);
                case MouseEventType.MouseExited: return OnMouseExit(args);
                case MouseEventType.MouseWheel: return OnMouseWheel(args);
                case MouseEventType.DoubleClick: return OnDoubleClick(args);
            }
            throw new NotImplementedException(String.Format("Mouse event type {0} does not have a handler method", eventType));
        }

        protected internal bool DispatchKeyPressEvent(char character)
        {
            EnsureNotDisposed();
            return OnKeyPress(character);
        }

        protected internal bool DispatchKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            EnsureNotDisposed();
            switch (type)
            {
                case KeyboardEventType.KeyDown: return OnKeyDown(args);
                case KeyboardEventType.KeyUp: return OnKeyUp(args);
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
        protected virtual bool OnMouseDown(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseDown, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse moving.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseMove(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseMoved, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button releasing.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseUp(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseUp, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse wheel scrolling. 
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseWheel(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseWheel, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse enter.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseEnter(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseEntered, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse exit.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseExit(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseExited, args);
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
            InvokeEventHandlers(DoubleClick, args);
            return true;
        }

        private void InvokeEventHandlers(Action<Responder, MouseEventArgs> handler, MouseEventArgs args)
        {
            EnsureNotDisposed();
            if (handler != null) handler(this, args);
        }
        #endregion

        #region Keyboard Events
        /// <summary>
        /// Calls the event handler for key pressing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnKeyDown(KeyboardEventArgs args)
        {
            InvokeEventHandlers(KeyDown, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for key releasing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnKeyUp(KeyboardEventArgs args)
        {
            InvokeEventHandlers(KeyUp, args);
            return true;
        }

        protected virtual bool OnKeyPress(char character)
        {
            InvokeEventHandlers(KeyPress, character);
            return true;
        }

        private void InvokeEventHandlers(Action<Responder, KeyboardEventArgs> handler, KeyboardEventArgs args)
        {
            if (handler != null) handler(this, args);
        }

        private void InvokeEventHandlers(Action<Responder, char> handler, char arg)
        {
            if (handler != null) handler(this, arg);
        }
        #endregion

        #region Update Events
        protected virtual void OnUpdate(UpdateEventArgs args)
        {
            EnsureNotDisposed();
            Action<Responder, UpdateEventArgs> handler = Updated;
            if (handler != null)
            {
                handler(this, args);
            }
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
                MouseDown = null;
                MouseUp = null;
                MouseMoved = null;
                MouseWheel = null;
                MouseEntered = null;
                MouseExited = null;
                KeyDown = null;
                KeyUp = null;
                KeyPress = null;
                Updated = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
