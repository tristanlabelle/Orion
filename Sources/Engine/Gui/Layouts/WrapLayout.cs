using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A layout <see cref="Control"/> which displays its items as a horizontal or vertical stack which wraps.
    /// </summary>
    public partial class WrapLayout : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private Direction direction = Direction.PositiveY;
        private int childGap;
        private int seriesGap;
        #endregion

        #region Constructors
        public WrapLayout()
        {
            children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Direction"/> of this <see cref="WrapLayout"/>,
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
        /// Accesses the gap between successive rows or columns of children, in pixels.
        /// </summary>
        public int SeriesGap
        {
            get { return seriesGap; }
            set
            {
                if (value == seriesGap) return;
                Argument.EnsurePositive(value, "SeriesGap");

                seriesGap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the collection of child <see cref="Control"/>s within this <see cref="WrapLayout"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Stacks a <see cref="Control"/> within this <see cref="WrapLayout"/>.
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
            return MeasureOrArrange(availableSize, false);
        }

        protected override void ArrangeChildren()
        {
            MeasureOrArrange(Size.Zero, true);
        }

        private Size MeasureOrArrange(Size availableSize, bool arrange)
        {
            Region rectangle = base.Rectangle;

            int x = 0;
            int y = 0;
            int firstChildInSeriesIndex = 0;
            int seriesSize = 0;
            int width = 0;
            int height = 0;

            for (int i = 0; i < children.Count; ++i)
            {
                Control child = children[i];

                Size availableChildSize;
                Size childSize;
                if (arrange)
                {
                    childSize = child.DesiredOuterSize;
                    availableChildSize = new Size(rectangle.Width - x, rectangle.Height - y);
                }
                else
                {
                    availableChildSize = new Size(availableSize.Width - x, availableSize.Height - y);
                    childSize = child.Measure(availableChildSize);
                }

                if (direction.IsHorizontal())
                {
                    if (i != 0 && childSize.Width > availableChildSize.Width)
                    {
                        if (arrange) ArrangeSeries(firstChildInSeriesIndex, i, y, seriesSize);
                        x = 0;
                        y += seriesGap + seriesSize;
                        seriesSize = 0;
                        firstChildInSeriesIndex = i;
                    }

                    if (childSize.Height > seriesSize) seriesSize = childSize.Height;

                    width = Math.Max(width, x + childGap + childSize.Width);
                    height = Math.Max(height, y + childSize.Height);

                    x += childSize.Width + childGap;
                }
                else
                {
                    if (i != 0 && childSize.Height > availableChildSize.Height)
                    {
                        if (arrange) ArrangeSeries(firstChildInSeriesIndex, i, x, seriesSize);
                        y = 0;
                        x += seriesGap + seriesSize;
                        seriesSize = 0;
                        firstChildInSeriesIndex = i;
                    }

                    if (childSize.Width > seriesSize) seriesSize = childSize.Width;

                    height = Math.Max(height, y + childGap + childSize.Height);
                    width = Math.Max(width, x + childSize.Width);

                    y += childSize.Height + childGap;
                }
            }

            if (arrange && children.Count > 0)
            {
                ArrangeSeries(firstChildInSeriesIndex, children.Count, direction.IsHorizontal() ? y : x, seriesSize);
            }

            return new Size(width, height);
        }

        private void ArrangeSeries(int firstIndex, int endIndex, int offset, int size)
        {
            Region rectangle = base.Rectangle;

            if (direction.IsHorizontal())
            {
                int x = 0;
                for (int i = firstIndex; i < endIndex; ++i)
                {
                    Control child = children[i];
                    Size childSize = child.DesiredOuterSize;
                    Region childRectangle = new Region(rectangle.MinX + x, rectangle.MinY + offset, childSize.Width, size);
                    DefaultArrangeChild(child, childRectangle);
                    x += childSize.Width + childGap;
                }
            }
            else
            {
                int y = 0;
                for (int i = firstIndex; i < endIndex; ++i)
                {
                    Control child = children[i];
                    Size childSize = child.DesiredOuterSize;
                    Region childRectangle = new Region(rectangle.MinX + offset, rectangle.MinY + y, size, childSize.Height);
                    DefaultArrangeChild(child, childRectangle);
                    y += childSize.Height + childGap;
                }
            }
        }
        #endregion
    }
}
