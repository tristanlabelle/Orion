using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A panel which displays its items as a horizontal or vertical stack.
    /// </summary>
    public sealed partial class StackPanel : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private Direction direction = Direction.MaxY;
        private int childGap;
        private int minChildSize;
        #endregion

        #region Constructors
        public StackPanel()
        {
            children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Direction"/> of this <see cref="StackPanel"/>,
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
        /// Gets the collection of child <see cref="Control"/>s within this <see cref="StackPanel"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Utility property to add children to this <see cref="StackPanel"/>.
        /// </summary>
        /// <remarks>
        /// This exists to leverage the object initializer feature of C# 3.
        /// </remarks>
        public IEnumerable<Control> InitChildren
        {
            set
            {
                Argument.EnsureNotNull(value, "InitChildren");

                foreach (Control control in value)
                    children.Add(control);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Stacks a <see cref="Control"/> within this <see cref="StackPanel"/>.
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

        protected override Size MeasureSize()
        {
            int width = 0;
            int height = 0;
            if (direction == Direction.MinY || direction == Gui2.Direction.MaxY)
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) height += childGap;

                    Size childSize = children[i].Measure();
                    height += Math.Max(minChildSize, childSize.Height);
                    if (childSize.Width > width) width = childSize.Width;
                }
            }
            else
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) width += childGap;

                    Size childSize = children[i].Measure();
                    width += Math.Max(minChildSize, childSize.Width);
                    if (childSize.Height > height) height = childSize.Height;
                }
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = base.Rectangle;

            if (direction == Direction.MinY || direction == Direction.MaxY)
            {
                int y = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) y += childGap;

                    Control child = children[i];
                    Size childSize = child.Measure();

                    int height = Math.Max(minChildSize, childSize.Height);
                    Region availableSpace = new Region(
                        rectangle.MinX,
                        direction == Direction.MaxY ? rectangle.MinY + y : rectangle.ExclusiveMaxY - height - y,
                        rectangle.Width, height);

                    Region childRectangle = DefaultArrange(availableSpace, child);
                    ArrangeChild(child, childRectangle);

                    y += height;
                }
            }
            else
            {
                int x = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) x += childGap;

                    Control child = children[i];
                    Size childSize = child.Measure();

                    int width = Math.Max(minChildSize, childSize.Width);
                    Region availableSpace = new Region(
                        direction == Direction.MaxX ? rectangle.MinX + x : rectangle.ExclusiveMaxX - width - x,
                        rectangle.MinY,
                        width, rectangle.Height);

                    Region childRectangle = DefaultArrange(availableSpace, child);
                    ArrangeChild(child, childRectangle);

                    x += width;
                }
            }
        }
        #endregion
    }
}
