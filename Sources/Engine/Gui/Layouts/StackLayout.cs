using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A layout <see cref="Control"/> which displays its items as a horizontal or vertical stack.
    /// </summary>
    public partial class StackLayout : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private Direction direction = Direction.PositiveY;
        private int childGap;
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
                    availableChildSize = Size.CreateClamped(rectangle.Width - x, rectangle.Height - y);
                }
                else
                {
                    availableChildSize = Size.CreateClamped(availableSize.Width - x, availableSize.Height - y);
                    childSize = child.Measure(availableChildSize);
                }

                if (direction.IsHorizontal())
                {
                    if (childSize.Height > seriesSize) seriesSize = childSize.Height;

                    width = Math.Max(width, x + childSize.Width);
                    height = Math.Max(height, y + childSize.Height);

                    if (arrange)
                    {
                        Region childRectangle = new Region(rectangle.MinX + x, rectangle.MinY + y, childSize.Width, availableChildSize.Height);
                        DefaultArrangeChild(child, childRectangle);
                    }

                    x += childSize.Width + childGap;
                }
                else
                {
                    if (childSize.Width > seriesSize) seriesSize = childSize.Width;

                    height = Math.Max(height, y + childSize.Height);
                    width = Math.Max(width, x + childSize.Width);

                    if (arrange)
                    {
                        Region childRectangle = new Region(rectangle.MinX + x, rectangle.MinY + y, availableChildSize.Width, childSize.Height);
                        DefaultArrangeChild(child, childRectangle);
                    }

                    y += childSize.Height + childGap;
                }
            }

            return new Size(width, height);
        }
        #endregion
    }
}
