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
        public virtual Rectangle Frame { get; set; }
        public virtual Rectangle Bounds { get; set; }

        public Vector2? MousePosition
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                return cursorPosition;
            }
            set
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                cursorPosition = value;
            }
        }

        public bool IsMouseOver
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                return cursorPosition.HasValue;
            }
        }
        #endregion

        #region Events
        public event GenericEventHandler<Responder, MouseEventArgs> MouseDown;
        public event GenericEventHandler<Responder, MouseEventArgs> MouseUp;
        public event GenericEventHandler<Responder, MouseEventArgs> MouseMoved;
        public event GenericEventHandler<Responder, MouseEventArgs> MouseWheel;
        public event GenericEventHandler<Responder, MouseEventArgs> MouseEntered;
        public event GenericEventHandler<Responder, MouseEventArgs> MouseExited;
        public event GenericEventHandler<Responder, KeyboardEventArgs> KeyDown;
        public event GenericEventHandler<Responder, KeyboardEventArgs> KeyUp;
        public event GenericEventHandler<Responder, UpdateEventArgs> Updated;
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
            if (isDisposed) throw new ObjectDisposedException(null);
            bool eventCanSink = true;
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                if (child.Frame.ContainsPoint(args.Position))
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
                else if (child.IsMouseOver)
                {
                    child.PropagateMouseEvent(MouseEventType.MouseExited, args);
                    child.MousePosition = null;
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
            if (isDisposed) throw new ObjectDisposedException(null);
            // for now, just propagate keyboard events to everyone, since more precise handling will require a focus system
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                child.PropagateKeyboardEvent(type, args);
                child.DispatchKeyboardEvent(type, args);
            }

            return DispatchKeyboardEvent(type, args);
        }

        /// <summary>Propagates an update event to the child views.</summary>
        /// <param name="args">The <see cref="UpdateEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal virtual void PropagateUpdateEvent(UpdateEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            foreach (Responder child in Enumerable.Reverse(Children))
            {
                child.PropagateUpdateEvent(args);
                child.OnUpdate(args);
            }
        }
        #endregion

        #region Event Dispatch
        protected internal bool DispatchMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            switch (eventType)
            {
                case MouseEventType.MouseDown: return OnMouseDown(args);
                case MouseEventType.MouseMoved: return OnMouseMove(args);
                case MouseEventType.MouseUp: return OnMouseUp(args);
                case MouseEventType.MouseEntered: return OnMouseEnter(args);
                case MouseEventType.MouseExited: return OnMouseExit(args);
                case MouseEventType.MouseWheel: return OnMouseWheel(args);
            }
            throw new NotImplementedException(String.Format("Mouse event type {0} does not have a handler method", eventType));
        }

        protected internal bool DispatchKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
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

        private void InvokeEventHandlers(GenericEventHandler<Responder, MouseEventArgs> handler, MouseEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            if (handler != null)
            {
                handler(this, args);
            }
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

        private void InvokeEventHandlers(GenericEventHandler<Responder, KeyboardEventArgs> handler, KeyboardEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            if (handler != null)
            {
                handler(this, args);
            }
        }
        #endregion

        #region Update Events
        protected virtual void OnUpdate(UpdateEventArgs args)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            GenericEventHandler<Responder, UpdateEventArgs> handler = Updated;
            if (handler != null)
            {
                handler(this, args);
            }
        }
        #endregion

        #region Hierarchy Events
        protected internal override void OnRemoveFromParent(ViewContainer parent)
        {
            if (cursorPosition.HasValue)
            {
                cursorPosition = null;
                PropagateMouseEvent(MouseEventType.MouseExited, new MouseEventArgs());
            }
            base.OnRemoveFromParent(parent);
        }
        #endregion

        #endregion

        public override void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException(null);

            MouseDown = null;
            MouseUp = null;
            MouseMoved = null;
            MouseWheel = null;
            MouseEntered = null;
            MouseExited = null;
            KeyDown = null;
            KeyUp = null;
            Updated = null;
            base.Dispose();
        }

        #endregion
    }
}
