using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="ContentControl"/> which enables scrolling its it contents.
    /// </summary>
    public class ScrollPanel : ContentControl
    {
        #region Fields
        private ScrollBar verticalScrollBar;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ScrollPanel"/>.
        /// </summary>
        public ScrollPanel()
        {
            verticalScrollBar = new ScrollBar();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="ScrollBar"/> that enables the user to scroll vertically.
        /// </summary>
        public ScrollBar VerticalScrollBar
        {
            get { return verticalScrollBar; }
        }
        #endregion

        #region Methods
        protected override IEnumerable<Control> GetChildren()
        {
            yield return verticalScrollBar;
            if (Content != null) yield return Content;
        }

        protected override Size MeasureInnerSize(Size availableSize)
        {
            int width = verticalScrollBar.Measure(availableSize).Width;
            int height = 0;

            if (Content != null)
            {
                Size contentSize = Content.Measure(
                    Size.CreateClamped(availableSize.Width - width, availableSize.Height - height));
                width += contentSize.Width;
                height += contentSize.Height;
            }

            return new Size(width, height);
        }

        protected override void ArrangeChildren()
        {
            Region rectangle = InnerRectangle;

            int verticalScrollBarWidth = Math.Min(verticalScrollBar.DesiredOuterSize.Width, rectangle.Width);
            verticalScrollBar.Arrange(new Region(
                rectangle.ExclusiveMaxX - verticalScrollBarWidth,
                rectangle.MinY, verticalScrollBarWidth, rectangle.Height));

            if (Content == null)
            {
                verticalScrollBar.Maximum = 1;
                verticalScrollBar.Length = 1;
            }
            else
            {
                verticalScrollBar.Maximum = Content.DesiredOuterSize.Height;
                verticalScrollBar.Length = rectangle.Height;

                int verticalScrollOffset = (int)verticalScrollBar.Value;

                Content.Arrange(new Region(
                    rectangle.MinX, rectangle.MinY - verticalScrollOffset,
                    rectangle.Width - verticalScrollBarWidth, Content.DesiredOuterSize.Height));
            }
        }
        #endregion
    }
}
