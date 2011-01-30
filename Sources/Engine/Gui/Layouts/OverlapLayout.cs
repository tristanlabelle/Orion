using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A layout <see cref="Control"/> which arranges its children so that each appears above the previous.
    /// </summary>
    public sealed partial class OverlapLayout : Control
    {
        #region Fields
        private readonly ChildCollection children;
        #endregion

        #region Constructors
        public OverlapLayout()
        {
            children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the children of this <see cref="OverlapLayout"/>. The last child is the topmost one.
        /// </summary>
        public new ChildCollection Children
        {
            get { return children; }
        }
        #endregion

        #region Methods
        public override Control GetChildAt(Point point)
        {
            for (int i = children.Count - 1; i >= 0; --i)
            {
                Control child = children[i];
                if (child.Rectangle.Contains(point)) return child;
            }

            return null;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return children;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            int width = 0;
            int height = 0;
            foreach (Control child in children)
            {
                Size childSize = child.Measure(availableSize);
                if (childSize.Width > width) width = childSize.Width;
                if (childSize.Height > height) height = childSize.Height;
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            foreach (Control child in children)
            {
                Region childRectangle = DefaultArrange(Rectangle, child);
                ArrangeChild(child, childRectangle);
            }
        }
        #endregion
    }
}
