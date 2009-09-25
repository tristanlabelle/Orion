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
        #region Fields
        private View parent;
        private readonly ViewChildrenCollection children;
        private Orion.Graphics.Drawing.GraphicsContext Context;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a view with a given frame. 
        /// </summary>
        /// <param name="frame">
        /// The <see cref="Rectangle"/> that will be this object's Frame and (by default) Bounds
        /// </param>
        public View(Rectangle frame)
        {
            Context = new Orion.Graphics.Drawing.GraphicsContext(new Rectangle(frame.Size));
            children = new ViewChildrenCollection(this);
            Frame = frame;
        }
        #endregion

        #region Events
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
        #endregion

        #region Properties
        /// <summary>
        /// Gets of this view. 
        /// </summary>
        public View Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        /// Gets the collection of this <see cref="View"/>'s children.
        /// </summary>
        public ViewChildrenCollection Children
        {
            get { return children; }
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
            get { return Context.CoordsSystem; }
            set { Context.CoordsSystem = value; }
        }
        #endregion

        #region Methods
        #region View Hierarchy
        /// <summary>
        /// Tests if this <see cref="View"/> is within the children of another <see cref="View"/>,
        /// recursively.
        /// </summary>
        /// <param name="other">The other <see cref="View"/> to test against.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="View"/> is a descendant of <paramref name="other"/>,
        /// <c>false</c> if not.
        /// </returns>
        public bool IsDescendantOf(View other)
        {
            Argument.EnsureNotNull(other, "other");
            while (other != null)
            {
                if (other == this) return true;
                other = other.Parent;
            }
            return false;
        }

        /// <summary>
        /// Tests if this <see cref="View"/> is a the parent of another <see cref="View"/>,
        /// recursively.
        /// </summary>
        /// <param name="other">The other <see cref="View"/> to test against.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="View"/> is an ancestor of <paramref name="other"/>,
        /// <c>false</c> if not.
        /// </returns>
        public bool IsAncestorOf(View other)
        {
            Argument.EnsureNotNull(other, "other");
            return other.IsAncestorOf(this);
        }

        /// <summary>
        /// Removes this <see cref="View"/> from its parent. 
        /// </summary>
        public void RemoveFromParent()
        {
            if (parent != null) parent.Children.Remove(this);
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
            foreach (View subview in Enumerable.Reverse(children))
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

        private void InvokeEventHandlers(GenericEventHandler<View, MouseEventArgs> handler, MouseEventArgs args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }
        #endregion

        #region Drawing
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

            foreach (View view in children)
            {
                view.Render();
            }
        }
        #endregion
        #endregion
    }
}
