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
        private Alignment itemAlignment = Alignment.Stretch;
        private int itemSpacing;
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

        public Alignment ItemAlignment
        {
            get { return itemAlignment; }
            set
            {
                Argument.EnsureDefined(value, "ItemAlignment");
                itemAlignment = value;
            }
        }

        public int ItemSpacing
        {
            get { return itemSpacing; }
            set
            {
                Argument.EnsurePositive(value, "ItemPadding");
                itemSpacing = value;
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
                    if (i > 0) height += itemSpacing;

                    Size childSize = children[i].Measure();
                    height += childSize.Height;
                    if (childSize.Width > width) width = childSize.Width;
                }
            }
            else
            {
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) width += itemSpacing;

                    Size childSize = children[i].Measure();
                    width += childSize.Width;
                    if (childSize.Height > height) height = childSize.Height;
                }
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = GetActualRectangle();

            if (orientation == Orientation.Vertical)
            {
                int internalWidth = Math.Max(rectangle.Width - Margin.MinX - Margin.MaxX, 0);

                int y = 0;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (i > 0) y += itemSpacing;

                    UIElement child = children[i];
                    Size childSize = child.Measure();

                    int x = 0;
                    int width = 0;
                    if (itemAlignment == Alignment.Center)
                    {
                        x = rectangle.MinX + Margin.MinX;
                        width = internalWidth;
                    }
                    else
                    {
                        width = Math.Min(childSize.Width, internalWidth);

                        if (itemAlignment == Alignment.Min)
                            x = rectangle.MinX + Margin.MinX;
                        else if (itemAlignment == Alignment.Max)
                            x = rectangle.ExclusiveMaxX - Margin.MaxX - width;
                        else if (itemAlignment == Alignment.Center)
                            x = (rectangle.MinX + Margin.MinX + rectangle.ExclusiveMaxX - Margin.MaxX) / 2 - width / 2;
                    }

                    SetChildRectangle(child, new Region(x, y, width, childSize.Height));

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
