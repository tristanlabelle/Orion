using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class for controls which have a single child control as content.
    /// </summary>
    public class ContentControl : Control
    {
        #region Fields
        private Control content;
        private Borders padding;
        #endregion

        #region Constructors
        public ContentControl() { }

        public ContentControl(Control content)
        {
            if (content != null)
            {
                AdoptChild(content);
                this.content = content;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Control"/> which forms the content of this <see cref="ContentControl"/>.
        /// </summary>
        public Control Content
        {
            get { return content; }
            set
            {
                if (value == content) return;
                if (value != null && value.Parent != null)
                    throw new ArgumentException("Cannot add a parented control as the content of this control.");

                if (content != null)
                {
                    Control oldContent = content;
                    content = null;
                    AbandonChild(oldContent);
                }

                if (value != null)
                {
                    AdoptChild(value);
                    content = value;
                }

                InvalidateMeasure();
            }
        }


        /// <summary>
        /// Gets the content rectangle of this <see cref="Control"/> which resulted from the last arrange pass.
        /// This value excludes the margins and the padding and is valid only if <see cref="IsArranged"/> is true.
        /// </summary>
        public Region InnerRectangle
        {
            get { return Borders.ShrinkClamped(Rectangle, Padding); }
        }

        #region Padding
        /// <summary>
        /// Accesses the padding between the borders of this <see cref="ContentControl"/> and its contents.
        /// </summary>
        public Borders Padding
        {
            get { return padding; }
            set
            {
                if (value == padding) return;

                padding = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the padding of this <see cref="Control"/> on the minimum X-axis side.
        /// </summary>
        public int MinXPadding
        {
            get { return padding.MinX; }
            set { Padding = new Borders(value, padding.MinY, padding.MaxX, padding.MaxY); }
        }

        /// <summary>
        /// Accesses the padding of this <see cref="Control"/> on the minimum Y-axis side.
        /// </summary>
        public int MinYPadding
        {
            get { return padding.MinY; }
            set { Padding = new Borders(padding.MinX, value, padding.MaxX, padding.MaxY); }
        }

        /// <summary>
        /// Accesses the padding of this <see cref="Control"/> on the maximum X-axis side.
        /// </summary>
        public int MaxXPadding
        {
            get { return padding.MaxX; }
            set { Padding = new Borders(padding.MinX, padding.MinY, value, padding.MaxY); }
        }

        /// <summary>
        /// Accesses the padding of this <see cref="Control"/> on the maximum Y-axis side.
        /// </summary>
        public int MaxYPadding
        {
            get { return padding.MaxY; }
            set { Padding = new Borders(padding.MinX, padding.MinY, padding.MaxX, value); }
        }

        /// <summary>
        /// Sets the width of the padding on the left and right of the <see cref="Control"/>.
        /// </summary>
        public int XPadding
        {
            set { Padding = new Borders(value, padding.MinY, value, padding.MaxY); }
        }

        /// <summary>
        /// Sets the height of the padding on the top and botton of the <see cref="Control"/>.
        /// </summary>
        public int YPadding
        {
            set { Padding = new Borders(padding.MinX, value, padding.MaxX, value); }
        }
        #endregion
        #endregion

        #region Methods
        protected override IEnumerable<Control> GetChildren()
        {
            if (content != null) yield return content;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            Size availableInnerSize = Size.CreateClamped(
                availableSize.Width - padding.TotalX,
                availableSize.Height - padding.TotalY);
            return MeasureInnerSize(availableInnerSize) + padding;
        }

        protected virtual Size MeasureInnerSize(Size availableSize)
        {
            if (content == null) return Size.Zero;

            return content.Measure(availableSize);
        }

        protected override void ArrangeChildren()
        {
            if (content != null) DefaultArrangeChild(content, InnerRectangle);
        }
        #endregion
    }
}
