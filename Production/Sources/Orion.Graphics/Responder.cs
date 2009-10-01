using System;

using Orion.Geometry;

namespace Orion.Graphics
{
	/// <summary>
	/// The Responder abstract class provides basic facilities for handling mouse and keyboard events for OpenGL-based graphics environments.
	/// </summary>
	public abstract class Responder
	{
		/// <summary>
		/// The local coordinate system of the responder.
		/// </summary>
		public abstract Rectangle Bounds { get; set; }
		
		/// <summary>
		/// The representation of the local coordinate system in the parent view's coordinate system.
		/// </summary>
		/// <remarks>
		/// When displayed, the Bounds contents scales to the Frame rectangle.
		/// </remarks>
		public abstract Rectangle Frame { get; set; }
		
        #region Event Properties
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
		/// The event triggered when the user presses a key.
		/// </summary>
		public event GenericEventHandler<Responder, KeyboardEventArgs> KeyDown;
		
		/// <summary>
		/// The event triggered when the user releases a key.
		/// </summary>
		public event GenericEventHandler<Responder, KeyboardEventArgs> KeyUp;
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
        /// Calls the appropriate event method for an event type
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
            }
            throw new NotImplementedException(String.Format("Mouse event type {0} does not have a handler method", eventType));
        }
		

        /// <summary>
        /// Calls the event handler for mouse clicks. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseClick(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseClicked, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button pressing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseDown(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseDown, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse moving. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseMove(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseMoved, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button releasing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseUp(MouseEventArgs args)
        {
            InvokeEventHandlers(MouseUp, args);
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
			switch(type)
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
		protected internal virtual bool OnKeyDown(KeyboardEventArgs args)
		{
			InvokeEventHandlers(KeyDown, args);
			return true;
		}
		
        /// <summary>
        /// Calls the event handler for key releasing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
		protected internal virtual bool OnKeyUp(KeyboardEventArgs args)
		{
			InvokeEventHandlers(KeyUp, args);
			return true;
		}
		
		private void InvokeEventHandlers(GenericEventHandler<Responder, KeyboardEventArgs> handler, KeyboardEventArgs args)
		{
			if(handler != null)
			{
				handler(this, args);
			}
		}
		
		#endregion
		
		#endregion
	}
}
