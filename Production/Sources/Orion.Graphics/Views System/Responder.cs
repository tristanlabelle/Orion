using System;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// The Responder abstract class provides basic facilities for handling mouse and keyboard events for OpenGL-based graphics environments.
    /// </summary>
    public abstract class Responder
    {
        #region Fields

        private Vector2? cursorPosition = null;

        #endregion

        #region Properties
        /// <summary>
        /// Indicates if the mouse is currently positioned over the Responder.
        /// </summary>
        public bool IsMouseOver
        {
            get { return cursorPosition.HasValue; }
        }

        /// <summary>
        /// Accesses the position, in local coordinates, of the mouse cursor over the view.
        /// </summary>
        public Vector2? CursorPosition
        {
            get { return cursorPosition; }
            set { cursorPosition = value; }
        }
        #endregion

        #region Events
        /// <summary>
        /// The event triggered when the user presses the left button of the mouse while positioned over the view
        /// </summary>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseDown;

        /// <summary>
        /// The event triggered when the user releases the left button of the mouse while positioned over the view
        /// </summary>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseUp;

        /// <summary>
        /// The event triggered when the used presses and releases the mouse button
        /// </summary>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseClicked;

        /// <summary>
        /// The event triggered when the user moves the mouse over the view
        /// </summary>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseMoved;
		
		/// <summary>
		/// The event triggered when the user scrolls using the mouse wheel. 
		/// </summary>
		public event GenericEventHandler<Responder, MouseEventArgs> MouseWheel;

        /// <summary>
        /// The event triggered when the mouse pointer enters the view region.
        /// </summary>
        /// <remarks>This event is typically followed by a MouseMoved event.</remarks>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseEntered;

        /// <summary>
        /// The event triggered when the mouse pointer exits the view region.
        /// </summary>
        public event GenericEventHandler<Responder, MouseEventArgs> MouseExited;

        /// <summary>
        /// The event triggered when the user presses a key.
        /// </summary>
        public event GenericEventHandler<Responder, KeyboardEventArgs> KeyDown;

        /// <summary>
        /// The event triggered when the user releases a key.
        /// </summary>
        public event GenericEventHandler<Responder, KeyboardEventArgs> KeyUp;

		/// <summary>
		/// The event triggered when the responder's state is updated.
		/// </summary>
		/// <remarks>This happens once in every run loop iteration.</remarks>
        public event GenericEventHandler<Responder, UpdateEventArgs> Updated;
        #endregion

        #region Event Handling

        #region Mouse Events
        /// <summary>
        /// Propagates a mouse event to the lower responder hierarchy.
        /// </summary>
        /// <param name="type">
        /// The <see cref="MouseEventType"/> type of the event
        /// </param>
        /// <param name="args">
        /// The event data itself
        /// </param>
        /// <returns>
        /// True if it's okay to propagate the event to the upper responder hierarchy.
        /// </returns>
        protected internal abstract bool PropagateMouseEvent(MouseEventType type, MouseEventArgs args);

        /// <summary>
        /// Calls the appropriate event method for an event type.
        /// </summary>
        /// <param name="eventType">The type of the event</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns></returns>
        protected internal bool DispatchMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            switch (eventType)
            {
                case MouseEventType.MouseClicked: return OnMouseClick(args);
                case MouseEventType.MouseDown: return OnMouseDown(args);
                case MouseEventType.MouseMoved: return OnMouseMove(args);
                case MouseEventType.MouseUp: return OnMouseUp(args);
                case MouseEventType.MouseEntered: return OnMouseEnter(args);
                case MouseEventType.MouseExited: return OnMouseExit(args);
			case MouseEventType.MouseWheel: return OnMouseWheel(args);
            }
            throw new NotImplementedException(String.Format("Mouse event type {0} does not have a handler method", eventType));
        }


        /// <summary>
        /// Calls the event handler for mouse clicks.
        /// </summary>
        /// <remarks>The default implementation allows for event sinking by always returning true.</remarks>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected virtual bool OnMouseClick(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseClicked, args);
            return true;
        }

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
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion

        #region Keyboard Events

        /// <summary>
        /// Propagates a keyboard event to the lower responder hierarchy.
        /// </summary>
        /// <param name="type">
        /// The <see cref="KeyboardEventType"/> of the event
        /// </param>
        /// <param name="args">
        /// The event data itself
        /// </param>
        /// <returns>
        /// True if it's okay to propagate the event to the upper responder hierarchy.
        /// </returns>
        protected internal abstract bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args);

        /// <summary>
        /// Dispatches a keyboard event to the correct method, based on the event type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="KeyboardEventType"/> of the event
        /// </param>
        /// <param name="args">
        /// The event data
        /// </param>
        /// <returns>
        /// True if it's okay to propagate the event to the upper responder hierarchy
        /// </returns>
        protected internal bool DispatchKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            switch (type)
            {
                case KeyboardEventType.KeyDown: return OnKeyDown(args);
                case KeyboardEventType.KeyUp: return OnKeyUp(args);
            }
            throw new NotImplementedException(String.Format("Keyboard event type {0} does not have a handler method", type));
        }


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
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion

        #region Update Event

        /// <summary>
        /// Calls the event handler for game run loop update.
        /// </summary>
        /// <param name="args">The update arguments passed to the events</param>
        protected virtual void OnUpdate(UpdateEventArgs args)
        {
            GenericEventHandler<Responder, UpdateEventArgs> handler = Updated;
            if (handler != null)
            {
                handler(this, args);
            }
        }

		/// <summary>
		/// Propagates the Update event handler to all child responders. 
		/// </summary>
		/// <param name="args">
		/// A <see cref="UpdateEventArgs"/> telling how much time passed since the last update event
		/// </param>
        protected internal abstract void PropagateUpdateEvent(UpdateEventArgs args);

        #endregion

        #endregion
    }
}
