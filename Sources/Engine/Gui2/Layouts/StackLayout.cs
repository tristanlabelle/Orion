using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A layout <see cref="Control"/> which displays its items as a horizontal or vertical stack.
    /// </summary>
    public partial class StackLayout : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private Direction direction = Direction.MaxY;
        private int childGap;
        private int minChildSize;
        #endregion

        #region Constructors
        public StackLayout()
        {
            children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Direction"/> of this <see cref="StackLayout"/>,
        /// which determines how child <see cref="Control"/>s are stacked.
        /// </summary>
        public Direction Direction
        {
            get { return direction; }
            set
            {
                if (value == direction) return;
                Argument.EnsureDefined(value, "Direction");

                direction = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the gap between successive child <see cref="Control"/>s, in pixels.
        /// </summary>
        public int ChildGap
        {
            get { return childGap; }
            set
            {
                if (value == childGap) return;
                Argument.EnsurePositive(value, "ChildGap");

                childGap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the minimum size of child <see cref="Control"/>s along the
        /// current <see cref="Orientation"/>.
        /// </summary>
        public int MinChildSize
        {
            get { return minChildSize; }
            set
            {
                if (value == minChildSize) return;
                Argument.EnsurePositive(value, "MinChildSize");

                minChildSize = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the collection of child <see cref="Control"/>s within this <see cref="StackLayout"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Stacks a <see cref="Control"/> within this <see cref="StackLayout"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be stacked.</param>
        public void Stack(Control control)
        {
            children.Add(control);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return children;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            var strategy = OrientationStrategy.FromDirection(direction);

            int width = 0;
            int height = 0;

            for (int i = 0; i < children.Count; ++i)
            {
                if (i > 0) strategy.IncrementPrimary(ref width, ref height, childGap);

                Size childSize = children[i].Measure(Size.MaxValue);

                int childLength = Math.Max(minChildSize, strategy.GetPrimary(childSize));
                strategy.IncrementPrimary(ref width, ref height, childLength);

                int childSecondarySizeComponent = strategy.GetSecondary(childSize);
                if (childSecondarySizeComponent > strategy.GetSecondary(width, height))
                    strategy.SetSecondary(ref width, ref height, childSecondarySizeComponent);
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = base.Rectangle;
            var strategy = OrientationStrategy.FromDirection(direction);

            int x = 0;
            int y = 0;
            for (int i = 0; i < children.Count; ++i)
            {
                if (i > 0) strategy.IncrementPrimary(ref x, ref y, childGap);

                Control child = children[i];
                Size childSize = child.DesiredOuterSize;

                int childLength = Math.Max(minChildSize, strategy.GetPrimary(childSize));
                Region availableSpace;
                if (strategy.Orientation == Orientation.Horizontal)
                {
                    availableSpace = new Region(
                        direction == Direction.MaxX ? rectangle.MinX + x : rectangle.ExclusiveMaxX - childLength - x,
                        rectangle.MinY,
                        childLength, rectangle.Height);
                }
                else
                {
                    availableSpace = new Region(
                        rectangle.MinX,
                        direction == Direction.MaxY ? rectangle.MinY + y : rectangle.ExclusiveMaxY - childLength - y,
                        rectangle.Width, childLength);
                }

                Region childRectangle = DefaultArrange(availableSpace, child);
                ArrangeChild(child, childRectangle);

                strategy.IncrementPrimary(ref x, ref y, childLength);
            }
        }
        #endregion
    }
}
