using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public sealed partial class StackPanel : Panel
    {
        #region Fields
        private readonly ChildCollection children;
        private Orientation orientation = Orientation.Vertical;
        private int itemGap;
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
                Argument.EnsureDefined(value, "Orientation");
                orientation = value;
            }
        }

        public int ItemGap
        {
            get { return itemGap; }
            set
            {
                Argument.EnsurePositive(value, "ItemPadding");
                itemGap = value;
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
                    if (i > 0) height += itemGap;

                    Size childSize = children[i].Measure();
                    height += childSize.Height;
                    if (childSize.Width > width) width = childSize.Width;
                }
            }
            else
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) width += itemGap;

                    Size childSize = children[i].Measure();
                    width += childSize.Width;
                    if (childSize.Height > height) height = childSize.Height;
                }
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = Arrange();

            if (orientation == Orientation.Vertical)
            {
                int internalWidth = Math.Max(rectangle.Width - Margin.MinX - Margin.MaxX, 0);

                int y = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) y += itemGap;

                    UIElement child = children[i];
                    Size childSize = child.Measure();

                    int x;
                    int width;
                    DefaultArrange(internalWidth, child.HorizontalAlignment, childSize.Width, out x, out width);

                    Region childRectangle = new Region(
                        rectangle.MinX + Margin.MinX + x,
                        rectangle.MinY + Margin.MinY + y,
                        width, childSize.Height);
                    SetChildRectangle(child, childRectangle);

                    y += childSize.Height;
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
