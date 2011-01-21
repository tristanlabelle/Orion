using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A layout <see cref="Control"/> which arranges its children along its edges.
    /// </summary>
    public partial class DockLayout : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private bool lastChildFill;
        #endregion

        #region Constructors
        public DockLayout()
        {
            this.children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if the last child <see cref="Control"/> of this <see cref="DockLayout"/>
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
        /// Gets the <see cref="Control"/> which fills the remaining space.
        /// </summary>
        public Control Filler
        {
            get { return lastChildFill && children.Count > 0 ? children[children.Count - 1].Control : null; }
        }

        /// <summary>
        /// Gets the collection of <see cref="Control"/> docked within this <see cref="DockLayout"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a given <see cref="Control"/> to this <see cref="DockLayout"/> with the specified <see cref="Dock"/> mode.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be added.</param>
        /// <param name="dock">The <see cref="Dock"/> mode of that <see cref="Control"/>.</param>
        public void Dock(Control control, Direction dock)
        {
            children.Add(control, dock);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return children.Select(dockedChild => dockedChild.Control);
        }

        protected override Size MeasureSize(Size availableSize)
        {
            int usedWidth = 0;
            int usedHeight = 0;
            int minWidth = 0;
            int minHeight = 0;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedControl child = children[i];

                Size availableChildSize = Size.CreateClamped(availableSize.Width - usedWidth, availableSize.Height - usedHeight);
                Size childSize = child.Control.Measure(availableChildSize);

                if (child.Dock.IsHorizontal())
                {
                    usedWidth += childSize.Width;
                    minHeight = Math.Max(minHeight, usedHeight + childSize.Height);
                }
                else
                {
                    usedHeight += childSize.Height;
                    minWidth = Math.Max(minWidth, usedWidth + childSize.Width);
                }
            }

            return new Size(
                Math.Max(usedWidth, minWidth),
                Math.Max(usedHeight, minHeight));
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = base.Rectangle;

            int remainingRectangleMinX = rectangle.MinX;
            int remainingRectangleMinY = rectangle.MinY;
            int remainingRectangleWidth = rectangle.Width;
            int remainingRectangleHeight = rectangle.Height;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedControl child = children[i];
                Size childSize = child.Control.DesiredOuterSize;

                int childRectangleMinX = remainingRectangleMinX;
                int childRectangleMinY = remainingRectangleMinY;
                int childRectangleWidth = remainingRectangleWidth;
                int childRectangleHeight = remainingRectangleHeight;

                if (!LastChildFill || i != children.Count - 1)
                {
                    switch (child.Dock)
                    {
                        case Direction.NegativeX:
                            childRectangleWidth = childSize.Width;
                            remainingRectangleMinX += childSize.Width;
                            remainingRectangleWidth -= childSize.Width;
                            break;

                        case Direction.NegativeY:
                            childRectangleHeight = childSize.Height;
                            remainingRectangleMinY += childSize.Height;
                            remainingRectangleHeight -= childSize.Height;
                            break;

                        case Direction.PositiveX:
                            childRectangleMinX = remainingRectangleMinX + remainingRectangleWidth - childSize.Width;
                            childRectangleWidth = childSize.Width;
                            remainingRectangleWidth -= childSize.Width;
                            break;

                        case Direction.PositiveY:
                            childRectangleMinY = remainingRectangleMinY + remainingRectangleHeight - childSize.Height;
                            childRectangleHeight = childSize.Height;
                            remainingRectangleHeight -= childSize.Height;
                            break;
                    }
                }

                if (childRectangleWidth <= 0 || childRectangleHeight <= 0)
                {
                    ArrangeChild(child.Control, new Region(childRectangleMinX, childRectangleMinY, 0, 0));
                    continue;
                }

                Region relativeChildRectangle = DefaultArrange(new Size(childRectangleWidth, childRectangleHeight), child.Control);
                Region childRectangle = new Region(
                    childRectangleMinX + relativeChildRectangle.MinX,
                    childRectangleMinY + relativeChildRectangle.MinY,
                    relativeChildRectangle.Width, relativeChildRectangle.Height);
                ArrangeChild(child.Control, childRectangle);
            }
        }
        #endregion
    }
}
