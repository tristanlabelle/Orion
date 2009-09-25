using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

using Orion.Graphics.Drawing;

namespace Orion.Graphics
{
    /// <summary>
    /// The class View is the base class for rendered components.
    /// Views are organized in a tree hierarchy, and each View has a list of sub-Views.
    /// When rendering, a View will first render itself, then render all of its subviews.
    /// </summary>
    /// <remarks>
    /// Views use a first-quadrant system coordinates, which means the origin is at the bottom left corner.
    /// </remarks>
    public abstract class View
    {
		private List<View> subviewsList;
		private Orion.Graphics.Drawing.GraphicsContext Context;
		
		/// <summary>
		/// The superview of this view. 
		/// </summary>
		public View Superview { get; private set; }
		
		/// <summary>
		/// The list of subviews this view has. 
		/// </summary>
		public IEnumerable<View> Subviews
		{
			get
			{
				return subviewsList.ToArray();
			}
		}
		
		/// <summary>
		/// The rectangle in which this view appears in its superview. May be different from the bounds.
		/// </summary>
		public virtual Rectangle Frame { get; set; }
		
		/// <summary>
		/// The internal coordinates system rectangle used for drawing.
		/// </summary>
        /// <remarks>Drawing is clamped to this rectangle.</remarks>
        public Rectangle Bounds
        {
            get
            {
                return Context.CoordsSystem;
            }

            set
            {
                Context.CoordsSystem = value;
            }
        }

        /// <summary>
        /// The event triggered when the user presses the left button of the mouse while positioned over the view
        /// </summary>
        public event GenericEventHandler<View, MouseEventArgs> MouseDown;

        /// <summary>
        /// The event triggered when the user releases the left button of the mouse while positioned over the view
        /// </summary>
        public event GenericEventHandler<View, MouseEventArgs> MouseUp;

        /// <summary>
        /// The event triggered when the used presses and releases the mouse button
        /// </summary>
        public event GenericEventHandler<View, MouseEventArgs> MouseClicked;

        /// <summary>
        /// The event triggered when the user moves the mouse over the view
        /// </summary>
        public event GenericEventHandler<View, MouseEventArgs> MouseMoved;
		
		/// <summary>
		/// Constructs a view with a given frame. 
		/// </summary>
		/// <param name="frame">
		/// The <see cref="Rectangle"/> that will be this object's Frame and (by default) Bounds
		/// </param>
		public View(Rectangle frame)
		{
			Context = new Orion.Graphics.Drawing.GraphicsContext(new Rectangle(frame.Size));
			subviewsList = new List<View>();
			Frame = frame;
        }

        #region View Hierarchy

        /// <summary>
		/// Inserts the given view in the view hierarchy under this one. 
		/// </summary>
		/// <param name="view">
		/// A <see cref="View"/>
		/// </param>
		/// <exception cref="ArgumentException">
		/// If the passed view already has a superview, or if the passed view contains this view somewhere down its hierarchy
		/// </exception>
		public void AddSubview(View view)
		{
			if(view.Superview != null)
			{
				throw new ArgumentException("Cannot add as a subview a view that's already in another superview");
			}
			
			if(view.ContainsSubview(this))
			{
				throw new ArgumentException("Cannot add a view as a subview to itself");
			}
			
			view.Superview = this;
			subviewsList.Add(view);
		}
		
		/// <summary>
		/// Removes a directly descendant view of this object from the view hierarchy. 
		/// </summary>
		/// <param name="subview">
		/// The direct descendant <see cref="View"/>
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// If the passed view is not a direct descendant of this one
		/// </exception> 
		public void RemoveSubview(View subview)
		{
			if(subview.Superview != this)
			{
				throw new ArgumentException("Cannot remove a subview whose superview is not this object");
			}
			
			subviewsList.Remove(subview);
			subview.Superview = null;
		}
		
		/// <summary>
		/// Removes the object from the view hierarchy. 
		/// </summary>
		public void RemoveFromSuperview()
		{
			Superview.RemoveSubview(this);
		}
		
		/// <summary>
		/// Indicates if the passed view is a descendant of this one. 
		/// </summary>
		/// <param name="view">
		/// The supposedly child view <see cref="View"/>
		/// </param>
		/// <returns>
		/// True if the passed view the same one as this one, or if it is under this one in the view hierarchy, false otherwise
		/// </returns>
		public bool ContainsSubview(View view)
		{
			while(view != null)
			{
				if(view == this)
					return true;
				view = view.Superview;
			}
			return false;
        }

        #endregion

        #region Event handling

        /// <summary>
        /// Propagates a mouse event to the subviews.
        /// Events are propagated in a bottom-up order, but priority of execution is given in a up-bottom order (that we will call "event sinking").
        /// </summary>
        /// <param name="eventType">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns>True if this view (and subviews) accepts to propagate events; false otherwise</returns>
        internal bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Context.SetUpGLContext(Frame);

            Matrix4 transformMatrix;
            Vector4 eventCoords = new Vector4(args.X, args.Y, 0, 1);

            GL.GetFloat(GetPName.ModelviewMatrix, out transformMatrix);
            Vector2 coords = Vector4.Transform(eventCoords, transformMatrix).Xy;

            bool eventSinking = true;
            foreach (View subview in Enumerable.Reverse(subviewsList))
            {
                if (subview.Frame.ContainsPoint(coords))
                {
                    eventSinking = subview.PropagateMouseEvent(eventType, args);
                    break;
                }
            }

            Context.RestoreGLContext();

            if (eventSinking)
            {
                return DispatchMouseEvent(eventType, args);
            }
            return false;
        }

        /// <summary>
        /// Calls the appropriate event method for an event type
        /// </summary>
        /// <param name="eventType">The type of the event</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns></returns>
        internal bool DispatchMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            switch (eventType)
            {
                case MouseEventType.MouseClicked: return OnMouseClick(args);
                case MouseEventType.MouseDown: return OnMouseDown(args);
                case MouseEventType.MouseMoved: return OnMouseMove(args);
                case MouseEventType.MouseUp: return OnMouseUp(args);
            }
            throw new ArgumentException("Event type {0} has no assigned dispatch method");
        }

        /// <summary>
        /// Calls the event handler for mouse clicks. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseClick(MouseEventArgs args)
        {
            HandleEvent(MouseClicked, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button pressing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseDown(MouseEventArgs args)
        {
            HandleEvent(MouseDown, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse moving. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseMove(MouseEventArgs args)
        {
            HandleEvent(MouseMoved, args);
            return true;
        }

        /// <summary>
        /// Calls the event handler for mouse button releasing. The default implementation allows for event sinking by always returning true.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> arguments</param>
        /// <returns>True if event sinking is allowed; false otherwise</returns>
        protected internal virtual bool OnMouseUp(MouseEventArgs args)
        {
            HandleEvent(MouseUp, args);
            return true;
        }

        private void HandleEvent(GenericEventHandler<View, MouseEventArgs> handler, MouseEventArgs args)
        {
            handler(this, args);
        }

        #endregion

        /// <summary>
        /// Renders the view inside the passed <see cref="Orion.Graphics.Drawing.GraphicsContext"/> object.
		/// </summary>
		/// <param name="context">
        /// A <see cref="Orion.Graphics.Drawing.GraphicsContext"/> on which the view can operate to render itself
		/// </param>
		protected abstract void Draw(Orion.Graphics.Drawing.GraphicsContext context);
		
		/// <summary>
		/// Renders the view hierarchy. 
		/// </summary>
		internal virtual void Render()
		{
			Context.SetUpGLContext(Frame);
			Draw(Context);
			Context.RestoreGLContext();
			
			foreach(View view in subviewsList)
			{
				view.Render();
			}
		}
    }
}
