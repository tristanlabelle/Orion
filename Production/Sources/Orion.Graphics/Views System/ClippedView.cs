using System;

using Orion.Geometry;

using OpenTK.Math;

namespace Orion.Graphics
{
    /// <summary>
    /// A ClippedView is a view whose full contents is not meant to be visible at once, and thus needs to be scrolled in some way.
    /// </summary>
    public abstract class ClippedView : View
    {
        /// <summary>
        /// Returns the maximum bounds of this object (in contrast to Bounds, which are the visible bounds of this object).
        /// </summary>
        public abstract Rectangle FullBounds { get; }
        private const float pixelScreenOverBoard = 5;
        private Vector2 BoundsLimitOuter = new Vector2(100, 75);
        private Vector2 BoundsLimitInner = new Vector2(10, 7);

        /// <summary>
        /// Constructs a new ClippedView.
        /// </summary>
        /// <param name="frame">The frame of the view</param>
        public ClippedView(Rectangle frame)
            : base(frame)
        { }

        /// <summary>
        /// Scales the view with a given ratio, revealing a greater or lesser area of the full bounds.
        /// </summary>
        /// <param name="factor">The factor by which to scale the view</param>
        public void Zoom(double factor)
        {
            Zoom(factor, Bounds.Center);
        }

        /// <summary>
        /// Scales the view with a given ratio, revealing a greater or lesser area of the full bounds.
        /// </summary>
        /// <param name="factor">The factor by which to scale the view</param>
        /// <param name="center">The point around which to scale</param>
        public void Zoom(double factor, Vector2 center)
        {
            Vector2 scale = new Vector2((float)factor, (float)factor);
            Vector2 newSize = Bounds.Size;
            Vector2 newOrigin = Bounds.Origin;
            newSize.Scale(scale);

            if (newSize.X > FullBounds.Size.X)
            {
                float ratio = Bounds.Size.Y / Bounds.Size.X;
                newSize.X = FullBounds.Size.X;
                newSize.Y = newSize.X * ratio;
            }
            if (newSize.Y > FullBounds.Size.Y)
            {
                float ratio = Bounds.Size.X / Bounds.Size.Y;
                newSize.Y = newSize.X * ratio;
                newSize.X = newSize.Y * ratio;
            }

            Rectangle newBounds = new Rectangle(newOrigin, newSize);

            Vector2 originDifference = Bounds.Center - newBounds.Center;
            originDifference.Scale(0.5f, 0.5f);
            newOrigin += originDifference;

            if (newOrigin.X < 0) newOrigin.X = 0;
            if (newOrigin.Y < 0) newOrigin.Y = 0;

            newBounds = new Rectangle(newOrigin, newSize);

            if (newBounds.MaxX > FullBounds.MaxX) newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;
            if (newBounds.MaxY > FullBounds.MaxY) newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;

            Bounds = new Rectangle(newOrigin, newSize);

        }
        /// <summary>
        /// Translates the bounds origin by given X and Y offsets. Checks the bounds so it's impossible to scroll past them.
        /// </summary>
        /// <param name="x">The abscissa offset to apply</param>
        /// <param name="y">The ordinates offset to apply</param>
        public void ScrollBy(double x, double y)
        {
            ScrollBy(new Vector2((float)x, (float)y));
        }

        /// <summary>
        /// Translates the bounds origin by a given vector. Checks the bounds so it's impossible to scroll past them.
        /// </summary>
        /// <param name="direction">The vector to apply to the bounds origin</param>
        public void ScrollBy(Vector2 direction)
        {
            Rectangle newBounds = Bounds.Translate(direction);
            Vector2 newOrigin = newBounds.Origin;
            Vector2 newSize = newBounds.Size;

            if (newOrigin.X < FullBounds.X)
                newOrigin.X = FullBounds.X;

            if (newOrigin.Y < FullBounds.Y)
                newOrigin.Y = FullBounds.Y;

            if (newBounds.MaxX > FullBounds.MaxX)
                newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;

            if (newBounds.MaxY > FullBounds.MaxY)
                newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;

            Bounds = new Rectangle(newOrigin, newSize);
        }
    }
}
