using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public sealed partial class StackPanel : UIElement
    {
        #region Fields
        private readonly ChildCollection children;
        private Orientation orientation = Orientation.Vertical;
        private Borders padding;
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
        
        public Borders Padding
        {
        	get { return padding; }
        	set
            {
                //if (value == padding) return;

                padding = value;
                InvalidateMeasure();
            }
        }

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
        #endregion

        #region Methods
        protected override ICollection<UIElement> GetChildren()
        {
            return children;
        }

        protected override Size MeasureWithoutMargin()
        {
            int width = 0;
            int height = 0;
            if (orientation == Orientation.Vertical)
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

            return new Size(width, height) + Padding;
        }

        protected override void ArrangeChildren()
        {
            Region rectangle;
            Region innerRectangle;
            if (!TryGetRectangle(out rectangle) || !Borders.TryShrink(rectangle, padding, out innerRectangle))
                return;

            if (orientation == Orientation.Vertical)
            {
                int y = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) y += childGap;

                    UIElement child = children[i];
                    Size childSize = child.Measure();

                    int x;
                    int width;
                    DefaultArrange(innerRectangle.Width, child.HorizontalAlignment, childSize.Width, out x, out width);

                    int height = Math.Max(minChildSize, childSize.Height);
                    Region childRectangle = new Region(
                        innerRectangle.MinX + Margin.MinX + x,
                        innerRectangle.MinY + Margin.MinY + y,
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
