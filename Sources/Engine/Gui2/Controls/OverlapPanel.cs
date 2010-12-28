using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2.Controls
{
    /// <summary>
    /// A panel control which arranges its children so that each appears above the previous.
    /// </summary>
    public sealed partial class OverlapPanel : Control
    {
        #region Fields
        private readonly ChildCollection children;
        #endregion

        #region Constructors
        public OverlapPanel()
        {
            children = new ChildCollection(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the children of this <see cref="OverlapPanel"/>. The last child is the topmost one.
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
                if (child.IsArranged && child.Rectangle.Contains(point))
                    return child;
            }

            return null;
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return children;
        }

        protected override Size MeasureSize()
        {
            int width = 0;
            int height = 0;
            foreach (Control child in children)
            {
                child.Measure();
                Size childSize = child.DesiredOuterSize;
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
