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
        private Orientation orientation = Orientation.Vertical;
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
        /// Accesses the <see cref="Orientation"/> of this <see cref="StackPanel"/>,
        /// which determines how child <see cref="Control"/>s are laid out.
        /// </summary>
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                if (value == orientation) return;
                Argument.EnsureDefined(value, "Orientation");

                orientation = value;
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
            if (orientation == Orientation.Vertical)
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) height += childGap;

                    Size childSize = children[i].MeasureOuterSize();
                    height += Math.Max(minChildSize, childSize.Height);
                    if (childSize.Width > width) width = childSize.Width;
                }
            }
            else
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) width += childGap;

                    Size childSize = children[i].MeasureOuterSize();
                    width += Math.Max(minChildSize, childSize.Width);
                    if (childSize.Height > height) height = childSize.Height;
                }
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle;
            if (!TryGetRectangle(out rectangle))
                return;

            if (orientation == Orientation.Vertical)
            {
                int y = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) y += childGap;

                    Control child = children[i];
                    Size childSize = child.MeasureOuterSize();

                    int x;
                    int width;
                    DefaultArrange(rectangle.Width, child.HorizontalAlignment, childSize.Width, out x, out width);

                    int height = Math.Max(minChildSize, childSize.Height);
                    Region childRectangle = new Region(
                        rectangle.MinX + Margin.MinX + x,
                        rectangle.MinY + Margin.MinY + y,
                        width, height);
                    SetChildOuterRectangle(child, childRectangle);

                    y += height;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
