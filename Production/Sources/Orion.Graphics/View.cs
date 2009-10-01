using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;

using Orion.Geometry;

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
    public abstract class View : ViewContainer
    {
        #region Fields
        internal readonly GraphicsContext context;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a view with a given frame. 
        /// </summary>
        /// <param name="frame">
        /// The <see cref="Rectangle"/> that will be this object's Frame and (by default) Bounds
        /// </param>
        public View(Rectangle frame)
            : base()
        {
            context = new GraphicsContext(new Rectangle(frame.Size));
            Frame = frame;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence of <see cref="View"/> which are ancestors of this one.
        /// </summary>
        public IEnumerable<ViewContainer> Ancestors
        {
            get
            {
                ViewContainer ancestor = Parent;
                while (ancestor != null)
                {
                    yield return ancestor;
                    ancestor = ancestor.Parent;
                }
            }
        }

        /// <summary>
        /// Gets the Z-index of this <see cref="View"/> within its parent. Bigger is higher.
        /// </summary>
        public int ZIndex
        {
            get
            {
                if (Parent == null) return 0;
                return Parent.Children.IndexOf(this);
            }
        }

        /// <summary>
        /// The rectangle in which this view appears in its superview. May be different from the bounds.
        /// </summary>
        public override Rectangle Frame { get; set; }

        /// <summary>
        /// The internal coordinate system rectangle used for drawing.
        /// </summary>
        /// <remarks>Drawing is clamped to this rectangle.</remarks>
        public override Rectangle Bounds
        {
            get { return context.CoordinateSystem; }
            set { context.CoordinateSystem = value; }
        }

        /// <summary>
        /// Gets the rectangle which bounds this <see cref="View"/> in top-level coordinates.
        /// </summary>
        public Rectangle AbsoluteFrame
        {
            get
            {
                if (Parent == null) return Frame;
                Rectangle parentFrame = Parent.AbsoluteFrame;
                Rectangle parentBounds = Parent.Bounds;

                return Rectangle.FromPoints(
                    parentFrame.Origin + parentBounds.LocalToParent(Frame.Origin),
                    parentFrame.Origin + parentBounds.LocalToParent(Frame.Max));
            }
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
        public bool IsAncestorOf(View other)
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

        #region Event handling
        /// <summary>
        /// Propagates a mouse event to the child views.
        /// </summary>
        /// <remarks>
        /// Events are propagated in a bottom-up order, but priority of execution is given in an up-bottom order (we will call this "event sinking").
        /// </remarks>
        /// <param name="eventType">The type of event to propagate</param>
        /// <param name="args">The event arguments as a <see cref="MouseEventArgs"/></param>
        /// <returns>True if this view (and its children) accepts to propagate events; false if they want to interrupt the event sinking</returns>
        protected internal sealed override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords -= Frame.Origin;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);
            coords += Bounds.Origin;

            return base.PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks));
        }

        #endregion

        #region Drawing
        /// <summary>
        /// This method is called when the view needs to render itself using its <see cref="GraphicsContext"/>.
        /// </summary>
        /// <remarks>
        /// When this methods is called, the <see cref="GraphicsContext"/> object, and the OpenGL control, are guaranteed to have been
        /// properly prepared for drawing. Attempts to draw outside of this method will throw an <see cref="System.InvalidOperationException"/>.
        /// </remarks>
        protected abstract void Draw();

        /// <summary>
        /// Renders the view hierarchy. 
        /// </summary>
        protected internal override sealed void Render()
        {
            context.SetUpGLContext(Frame);

            Draw();

            base.Render();

            context.RestoreGLContext();
        }
        #endregion
        #endregion
    }
}
