using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Objects of this class are enabled to contain other ViewContainer objects to establish an event handling and drawing hierarchy.
    /// </summary>
    public abstract class ViewContainer : Responder
    {
        #region Fields
        private ViewChildrenCollection children;
        private ViewContainer parent;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a ViewContainer.
        /// </summary>
        public ViewContainer()
        {
            children = new ViewChildrenCollection(this);
        }

        #endregion

        #region Properties

        #region Frames and Bounds
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

        /// <summary>
        /// Gets the rectangle which bounds this <see cref="View"/> in top-level coordinates.
        /// </summary>
        public Rectangle AbsoluteFrame
        {
            get
            {
                if (parent == null) return Frame;
                Rectangle parentFrame = parent.AbsoluteFrame;
                Rectangle parentBounds = parent.Bounds;

                return Rectangle.FromPoints(
                    parentFrame.Origin + parentBounds.LocalToParent(Frame.Origin),
                    parentFrame.Origin + parentBounds.LocalToParent(Frame.Max));
            }
        }
        #endregion

        #region Genealogy

        /// <summary>
        /// Gets the parent ViewContainer of this container.
        /// </summary>
        public ViewContainer Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets the collection of this <see cref="View"/>'s children.
        /// </summary>
        public ViewChildrenCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets the sequence of <see cref="View"/> which are descendants of this one.
        /// </summary>
        public IEnumerable<ViewContainer> Descendants
        {
            get
            {
                foreach (ViewContainer child in children)
                {
                    yield return child;
                    foreach (View childDescendant in child.Descendants)
                    {
                        yield return childDescendant;
                    }
                }
            }
        }

        /// <summary>
        /// Tests if this <see cref="View"/> is within the children of another <see cref="View"/>,
        /// recursively.
        /// </summary>
        /// <param name="other">The other <see cref="View"/> to test against.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="View"/> is a descendant of <paramref name="other"/>,
        /// <c>false</c> if not.
        /// </returns>
        public bool IsDescendantOf(ViewContainer other)
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
        public bool IsAncestorOf(ViewContainer other)
        {
            Argument.EnsureNotNull(other, "other");
            return other.IsDescendantOf(this);
        }

        /// <summary>
        /// Removes this <see cref="View"/> from its parent. 
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent != null) Parent.Children.Remove(this);
        }

        #endregion

        #endregion

        #region Methods

        #region Event Handling

        /// <summary>
        /// Propagates a mouse event to the child views.
        /// </summary>
        /// <remarks>
        /// Events are propagated in a bottom-up order, but priority of execution is given in an up-bottom order (we will call this "event sinking").
        /// </remarks>
        /// <param name="type">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal override bool PropagateMouseEvent(MouseEventType type, MouseEventArgs args)
        {
            bool eventCanSink = true;
            foreach (View child in Enumerable.Reverse(children))
            {
                if (child.Frame.ContainsPoint(args.Position))
                {
                    if (eventCanSink)
                    {
                        if (!child.IsMouseOver)
                        {
                            child.DispatchMouseEvent(MouseEventType.MouseEntered, args);
                            child.IsMouseOver = true;
                        }
                        eventCanSink &= child.PropagateMouseEvent(type, args);
                    }
                }
                else if(child.IsMouseOver)
                {
                    child.DispatchMouseEvent(MouseEventType.MouseExited, args);
                    child.IsMouseOver = false;
                }
            }

            if (eventCanSink)
            {
                return DispatchMouseEvent(type, args);
            }
            return false;
        }

        /// <summary>
        /// Propagates a keyboard event to the child views.
        /// </summary>
        /// <remarks>
        /// Events are propagated in a bottom-up order, but priority of execution is given in an up-bottom order (we will call this "event sinking").
        /// </remarks>
        /// <param name="type">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="KeyboardEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal override bool PropagateKeyboardEvent(KeyboardEventType type, KeyboardEventArgs args)
        {
            // for now, just propagate keyboard events to everyone, since more precise handling will require a focus system
            foreach (ViewContainer child in Enumerable.Reverse(children))
            {
                child.DispatchKeyboardEvent(type, args);
            }

            return DispatchKeyboardEvent(type, args);
        }

        protected internal override void PropagateUpdateEvent(UpdateEventArgs args)
        {
            foreach (ViewContainer child in Enumerable.Reverse(children))
            {
                child.PropagateUpdateEvent(args);
                child.OnUpdate(args);
            }
        }

        #endregion

        #region Drawing
        /// <summary>
        /// Renders this container.
        /// </summary>
        protected internal virtual void Render()
        {
            foreach (ViewContainer container in Children)
            {
                container.Render();
            }
        }

        #endregion

        #endregion
    }
}
