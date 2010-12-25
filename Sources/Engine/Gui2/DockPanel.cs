using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used to prevent naming clashes with the Dock method.
using DockEnum = Orion.Engine.Gui2.Direction;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A panel which arranges its children along its edges.
    /// </summary>
    public sealed partial class DockPanel : Control
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
        /// Accesses a value indicating if the last child <see cref="Control"/> of this <see cref="DockPanel"/>
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
        /// Gets the collection of <see cref="Control"/> docked within this <see cref="DockPanel"/>.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Convenience setter to assign initial children to this <see cref="Control"/>.
        /// This operation may not be supported by the actual <see cref="Control"/> type.
        /// </summary>
        public IEnumerable<DockedControl> InitChildren
        {
            set
            {
                foreach (DockedControl control in value)
                    children.Add(control);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a given <see cref="Control"/> to this <see cref="DockPanel"/> with the specified <see cref="Dock"/> mode.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be added.</param>
        /// <param name="dock">The <see cref="Dock"/> mode of that <see cref="Control"/>.</param>
        public void Dock(Control control, Direction dock)
        {
            children.Add(control, dock);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return children.Controls;
        }

        protected override Size MeasureSize()
        {
            int usedWidth = 0;
            int usedHeight = 0;
            int freeWidth = 0;
            int freeHeight = 0;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedControl child = children[i];
                Size childSize = child.Control.MeasureOuterSize();

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
            Region rectangle;
            if (!TryGetRectangle(out rectangle)) return;

            int remainingRectangleMinX = rectangle.MinX;
            int remainingRectangleMinY = rectangle.MinY;
            int remainingRectangleWidth = rectangle.Width;
            int remainingRectangleHeight = rectangle.Height;

            for (int i = 0; i < children.Count; ++i)
            {
                DockedControl child = children[i];
                Size childSize = child.Control.MeasureOuterSize();

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

                if (childRectangleWidth <= 0 || childRectangleHeight <= 0)
                {
                    SetChildOuterRectangle(child.Control, null);
                    continue;
                }

                Region relativeChildRectangle = DefaultArrange(new Size(childRectangleWidth, childRectangleHeight), child.Control);
                Region childRectangle = new Region(
                    childRectangleMinX + relativeChildRectangle.MinX,
                    childRectangleMinY + relativeChildRectangle.MinY,
                    relativeChildRectangle.Width, relativeChildRectangle.Height);
                SetChildOuterRectangle(child.Control, childRectangle);
            }
        }
        #endregion
    }
}
