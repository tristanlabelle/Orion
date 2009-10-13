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
            set
            {
                context.CoordinateSystem = value;
                if (IsMouseOver)
                {
                    Vector2 position = CursorPosition.Value;
                    PropagateMouseEvent(MouseEventType.MouseMoved, new MouseEventArgs(position.X, position.Y, MouseButton.None, 0, 0));
                }
            }
        }

        #endregion

        #region Methods

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

            return base.PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
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
