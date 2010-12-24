using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used to prevent naming clashes with the Dock method.
using DockEnum = Orion.Engine.Gui2.Dock;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A panel which arranges its children along its edges.
    /// </summary>
    public sealed partial class DockPanel : UIElement
    {
        #region Fields
        private readonly ChildCollection children;
        private bool lastChildFill;
        #endregion

        #region Constructors
        public DockPanel()
        {
            this.children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if the last child <see cref="UIElement"/> of this <see cref="DockPanel"/>
        /// should be stretched to fill the remaining space, regardless of its <see cref="Dock"/> value.
        /// </summary>
        public bool LastChildFill
        {
            get { return lastChildFill; }
            set
            {
                if (value == lastChildFill) return;
                lastChildFill = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the <see cref="UIElement"/> which fills the remaining space.
        /// </summary>
        public UIElement Filler
        {
            get { return lastChildFill && children.Count > 0 ? children[children.Count - 1].Element : null; }
        }

        /// <summary>
        /// Gets the collection of <see cref="UIElement"/> docked within this <see cref="DockPanel"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Convenience setter to assign initial children to this <see cref="UIElement"/>.
        /// This operation may not be supported by the actual <see cref="UIElement"/> type.
        /// </summary>
        public new IEnumerable<DockedElement> InitChildren
        {
            set
            {
                foreach (DockedElement element in value)
                    children.Add(element);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a given <see cref="UIElement"/> to this <see cref="DockPanel"/> with the specified <see cref="Dock"/> mode.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to be added.</param>
        /// <param name="dock">The <see cref="Dock"/> mode of that <see cref="UIElement"/>.</param>
        public void Dock(UIElement element, Dock dock)
        {
            children.Add(element, dock);
        }

        protected override ICollection<UIElement> GetChildren()
        {
            return children.Elements;
        }

        protected override Size MeasureWithoutMargin()
        {
            int usedWidth = 0;
            int usedHeight = 0;
            int freeWidth = 0;
            int freeHeight = 0;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedElement child = children[i];
                Size childSize = child.Element.Measure();

                if (LastChildFill && i == children.Count - 1)
                {
                    if (childSize.Width > freeWidth) usedWidth += childSize.Width - freeWidth;
                    if (childSize.Height > freeHeight) usedHeight += childSize.Height - freeHeight;
                    break;
                }

                if (child.Dock == DockEnum.MinX || child.Dock == DockEnum.MaxX)
                {
                    if (childSize.Width > freeWidth)
                    {
                        usedWidth += childSize.Width - freeWidth;
                        freeWidth = 0;
                    }
                    else freeWidth -= childSize.Width;

                    if (childSize.Height > freeHeight)
                    {
                        int heightDelta = childSize.Height - freeHeight;
                        usedHeight += heightDelta;
                        freeHeight += heightDelta;
                    }
                }
                else
                {
                    if (childSize.Height > freeHeight)
                    {
                        usedHeight += childSize.Height - freeHeight;
                        freeHeight = 0;
                    }
                    else freeHeight -= childSize.Height;

                    if (childSize.Width > freeWidth)
                    {
                        int widthDelta = childSize.Width - freeWidth;
                        usedWidth += widthDelta;
                        freeWidth += widthDelta;
                    }
                }
            }

            return new Size(usedWidth, usedHeight);
        }

        protected override void ArrangeChildren()
        {
            Region? childrenBounds = GetReservedRectangle() - Margin;
            if (!childrenBounds.HasValue) return;

            int remainingRectangleMinX = childrenBounds.Value.MinX;
            int remainingRectangleMinY = childrenBounds.Value.MinY;
            int remainingRectangleWidth = childrenBounds.Value.Width;
            int remainingRectangleHeight = childrenBounds.Value.Height;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedElement child = children[i];
                Size childSize = child.Element.Measure();

                int childRectangleMinX = remainingRectangleMinX;
                int childRectangleMinY = remainingRectangleMinY;
                int childRectangleWidth = remainingRectangleWidth;
                int childRectangleHeight = remainingRectangleHeight;

                if (!LastChildFill || i != children.Count - 1)
                {
                    switch(child.Dock)
                    {
                        case DockEnum.MinX:
                            childRectangleWidth = childSize.Width;
                            remainingRectangleMinX += childSize.Width;
                            remainingRectangleWidth -= childSize.Width;
                            break;

                        case DockEnum.MinY:
                            childRectangleHeight = childSize.Height;
                            remainingRectangleMinY += childSize.Height;
                            remainingRectangleHeight -= childSize.Height;
                            break;

                        case DockEnum.MaxX:
                            childRectangleMinX = remainingRectangleMinX + remainingRectangleWidth - childSize.Width;
                            childRectangleWidth = childSize.Width;
                            remainingRectangleWidth -= childSize.Width;
                            break;

                        case DockEnum.MaxY:
                            childRectangleMinY = remainingRectangleMinY + remainingRectangleHeight - childSize.Height;
                            childRectangleHeight = childSize.Height;
                            remainingRectangleHeight -= childSize.Height;
                            break;
                    }
                }

                Region relativeChildRectangle = DefaultArrange(new Size(childRectangleWidth, childRectangleHeight), child.Element);
                Region childRectangle = new Region(
                    childRectangleMinX + relativeChildRectangle.MinX,
                    childRectangleMinY + relativeChildRectangle.MinY,
                    relativeChildRectangle.Width, relativeChildRectangle.Height);
                SetChildRectangle(child.Element, childRectangle);
            }
        }
        #endregion
    }
}
