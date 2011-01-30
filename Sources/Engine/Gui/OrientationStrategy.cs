using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Supports the implementation of generic algorithms that work in both horizontal and vertical orientations.
    /// </summary>
    public abstract class OrientationStrategy
    {
        #region Implementations
        private sealed class HorizontalStrategy : OrientationStrategy
        {
            public override Orientation Orientation
            {
                get { return Orientation.Horizontal; }
            }

            public override int GetPrimary(int x, int y) { return x; }
            public override void SetPrimary(ref int x, ref int y, int value) { x = value; }
        }

        private sealed class VerticalStrategy : OrientationStrategy
        {
            public override Orientation Orientation
            {
                get { return Orientation.Vertical; }
            }

            public override int GetPrimary(int x, int y) { return y; }
            public override void SetPrimary(ref int x, ref int y, int value) { y = value; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The horizontal <see cref="OrientationStrategy"/>.
        /// </summary>
        public static readonly OrientationStrategy Horizontal = new HorizontalStrategy();

        /// <summary>
        /// The vertical <see cref="OrientationStrategy"/>.
        /// </summary>
        public static readonly OrientationStrategy Vertical = new VerticalStrategy();
        #endregion

        #region Constructors
        internal OrientationStrategy() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the orientation of this <see cref="OrientationStrategy"/>.
        /// </summary>
        public abstract Orientation Orientation { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the <see cref="OrientationStrategy"/> which corresponds to a given <see cref="Orientation"/>.
        /// </summary>
        /// <param name="orientation">The <see cref="Orientation"/> for which the <see cref="OrientationStrategy"/> is required.</param>
        /// <returns>The <see cref="OrientationStrategy"/> for that <see cref="Orientation"/>.</returns>
        public static OrientationStrategy FromOrientation(Orientation orientation)
        {
            return orientation == Orientation.Horizontal ? Horizontal : Vertical;
        }

        /// <summary>
        /// Gets the <see cref="OrientationStrategy"/> which corresponds to a given <see cref="Direction"/>.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> for which the <see cref="OrientationStrategy"/> is required.</param>
        /// <returns>The <see cref="OrientationStrategy"/> for that <see cref="Direction"/>.</returns>
        public static OrientationStrategy FromDirection(Direction direction)
        {
            return FromOrientation(direction.GetOrientation());
        }

        /// <summary>
        /// Gets the primary component of the given size.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <returns><paramref name="x"/> for the horizontal strategy, <paramref name="y"/> for the vertical one.</returns>
        public abstract int GetPrimary(int x, int y);

        /// <summary>
        /// Gets the secondary component of a given size.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <returns><paramref name="x"/> for the vertical strategy, <paramref name="y"/> for the horizontal one.</returns>
        public int GetSecondary(int x, int y)
        {
            return GetPrimary(y, x);
        }

        /// <summary>
        /// Gets the primary component of the given <see cref="Size"/>.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The width for the horizontal strategy, or the height for the vertical one.</returns>
        public int GetPrimary(Size size)
        {
            return GetPrimary(size.Width, size.Height);
        }

        /// <summary>
        /// Gets the primary component of the given <see cref="Size"/>.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The width for the vertical strategy, or the height for the horizontal one.</returns>
        public int GetSecondary(Size size)
        {
            return GetPrimary(size.Height, size.Width);
        }

        /// <summary>
        /// Gets the primary component of the given <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The x coordinate for the horizontal strategy, or the y coordinate for the vertical one.</returns>
        public int GetPrimary(Point point)
        {
            return GetPrimary(point.X, point.Y);
        }

        /// <summary>
        /// Gets the secondary component of the given <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The x coordinate for the vertical strategy, or the y coordinate for the horizontal one.</returns>
        public int GetSecondary(Point point)
        {
            return GetPrimary(point.Y, point.X);
        }

        /// <summary>
        /// Sets the primary size component.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="value">
        /// The new value of <paramref name="x"/> for the horizontal strategy,
        /// or <paramref name="y"/> for the vertical one.
        /// </param>
        public abstract void SetPrimary(ref int x, ref int y, int value);

        /// <summary>
        /// Sets the secondary size component.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="value">
        /// The new value of <paramref name="x"/> for the vertical strategy,
        /// or <paramref name="y"/> for the horizontal one.
        /// </param>
        public void SetSecondary(ref int x, ref int y, int value)
        {
            SetPrimary(ref y, ref x, value);
        }

        /// <summary>
        /// Increments the primary size component.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="amount">The amount to increment the primary component by.</param>
        public void IncrementPrimary(ref int x, ref int y, int amount)
        {
            SetPrimary(ref x, ref y, GetPrimary(x, y) + amount);
        }

        /// <summary>
        /// Increments the secondary size component.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="amount">The amount to increment the secondary component by.</param>
        public void IncrementSecondary(ref int x, ref int y, int amount)
        {
            SetPrimary(ref y, ref x, GetPrimary(y, x) + amount);
        }
        #endregion
    }
}
