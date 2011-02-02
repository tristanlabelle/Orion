using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    // This class part defines members relating to the layout system and its measure and arrange phases.
    partial class Control
    {
        #region Fields
        private Borders margin;
        private Alignment horizontalAlignment;
        private Alignment verticalAlignment;
        private Size minSize;
        private int? width;
        private int? height;

        /// <summary>
        /// A cached value of the optimal space for this <see cref="Control"/> based on the size of its contents.
        /// This value is only meaningful if the control has been measured.
        /// </summary>
        private Size cachedDesiredOuterSize;

        /// <summary>
        /// A cached value of the client space rectangle reserved for this <see cref="Control"/>.
        /// This value is only meaningful if the control has been arranged.
        /// </summary>
        private Region cachedOuterRectangle;

        private bool isMeasured;
        private bool isArranged;
        #endregion

        #region Properties
        #region Margin
        /// <summary>
        /// Accesses the margins around this <see cref="Control"/>.
        /// </summary>
        public Borders Margin
        {
            get { return margin; }
            set
            {
                this.margin = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="Control"/> on the minimum X-axis side.
        /// </summary>
        public int MinXMargin
        {
            get { return margin.MinX; }
            set { Margin = new Borders(value, margin.MinY, margin.MaxX, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="Control"/> on the minimum Y-axis side.
        /// </summary>
        public int MinYMargin
        {
            get { return margin.MinY; }
            set { Margin = new Borders(margin.MinX, value, margin.MaxX, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="Control"/> on the maximum X-axis side.
        /// </summary>
        public int MaxXMargin
        {
            get { return margin.MaxX; }
            set { Margin = new Borders(margin.MinX, margin.MinY, value, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="Control"/> on the maximum Y-axis side.
        /// </summary>
        public int MaxYMargin
        {
            get { return margin.MaxY; }
            set { Margin = new Borders(margin.MinX, margin.MinY, margin.MaxX, value); }
        }

        /// <summary>
        /// Sets the width of the margin on the left and right of the <see cref="Control"/>.
        /// </summary>
        public int XMargin
        {
            set { Margin = new Borders(value, margin.MinY, value, margin.MaxY); }
        }

        /// <summary>
        /// Sets the height of the margin on the top and botton of the <see cref="Control"/>.
        /// </summary>
        public int YMargin
        {
            set { Margin = new Borders(margin.MinX, value, margin.MaxX, value); }
        }
        #endregion

        #region Alignment
        /// <summary>
        /// Accesses the horizontal alignment hint for this <see cref="Control"/>.
        /// The parent <see cref="Control"/> is charged of honoring or not this value.
        /// </summary>
        public Alignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set
            {
                if (value == horizontalAlignment) return;
                Argument.EnsureDefined(value, "HorizontalAlignment");

                horizontalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Accesses the vertical alignment hint for this <see cref="Control"/>.
        /// The parent <see cref="Control"/> is charged of honoring or not this value.
        /// </summary>
        public Alignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set
            {
                if (value == verticalAlignment) return;
                Argument.EnsureDefined(value, "VerticalAlignment");

                verticalAlignment = value;
                InvalidateArrange();
            }
        }
        #endregion

        #region Size
        /// <summary>
        /// Accesses the minimum size of this <see cref="Control"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="Control"/>.
        /// </summary>
        public Size MinSize
        {
            get { return minSize; }
            set
            {
                if (value == minSize) return;

                minSize = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the minimum width of this <see cref="Control"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="Control"/>.
        /// </summary>
        public int MinWidth
        {
            get { return minSize.Width; }
            set
            {
                Argument.EnsurePositive(value, "MinimumWidth");
                MinSize = new Size(value, minSize.Height);
            }
        }

        /// <summary>
        /// Accesses the minimum width of this <see cref="Control"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="Control"/>.
        /// </summary>
        public int MinHeight
        {
            get { return minSize.Height; }
            set
            {
                Argument.EnsurePositive(value, "MinimumWHeight");
                MinSize = new Size(minSize.Width, value);
            }
        }

        /// <summary>
        /// Accesses the target width for this <see cref="Control"/>, this excludes the margins.
        /// A value of <c>null</c> indicates that the <see cref="Control"/> should use default sizing behavior.
        /// <see cref="MinHeight"/> takes precedence on this value.
        /// </summary>
        public int? Width
        {
            get { return width; }
            set
            {
                if (value == width) return;
                if (value.HasValue) Argument.EnsurePositive(value.Value, "Width");

                this.width = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the target height for this <see cref="Control"/>, this excludes the margins.
        /// A value of <c>null</c> indicates that the <see cref="Control"/> should use default sizing behavior.
        /// <see cref="MinHeight"/> takes precedence on this value.
        /// </summary>
        public int? Height
        {
            get { return height; }
            set
            {
                if (value == height) return;
                if (value.HasValue) Argument.EnsurePositive(value.Value, "Height");

                this.height = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the desired outer size of this <see cref="Control"/> which resulted from the last measure pass.
        /// This value includes the margins and is valid only if <see cref="IsMeasured"/> is true.
        /// </summary>
        public Size DesiredOuterSize
        {
            get { return cachedDesiredOuterSize; }
        }


        /// <summary>
        /// Gets the desired size of this <see cref="Control"/> which resulted from the last measure pass.
        /// This value excludes the margins and is valid only if <see cref="IsMeasured"/> is true.
        /// </summary>
        public Size DesiredSize
        {
            get { return Borders.ShrinkClamped(cachedDesiredOuterSize, margin); }
        }

        /// <summary>
        /// Gets the actual size of this <see cref="Control"/> which resulted from the last arrange pass.
        /// This value excludes the margins and is valid only if <see cref="IsArranged"/> is true.
        /// </summary>
        public Size ActualSize
        {
            get { return Rectangle.Size; }
        }
        #endregion

        #region Rectangle
        /// <summary>
        /// Gets the outer rectangle bounding this <see cref="Control"/> which resulted from the last arrange pass.
        /// This value includes the margins and is valid only if <see cref="IsArranged"/> is true.
        /// </summary>
        public Region OuterRectangle
        {
            get { return cachedOuterRectangle; }
        }

        /// <summary>
        /// Gets the rectangle bounding this <see cref="Control"/> which resulted from the last arrange pass.
        /// This value excludes the margins and is valid only if <see cref="IsArranged"/> is true.
        /// </summary>
        public Region Rectangle
        {
            get { return Borders.ShrinkClamped(cachedOuterRectangle, margin); }
        }
        #endregion

        #region Layout State
        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> has been measured.
        /// If this is <c>true</c>, the value of <see cref="DesiredOuterSize"/> is valid.
        /// </summary>
        public bool IsMeasured
        {
            get { return isMeasured; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> has been arranged.
        /// If this is <c>true</c>, the values of <see cref="OuterRectangle"/>,
        /// <see cref="Rectangle"/> and <see cref="ActualSize"/> are valid.
        /// </summary>
        public bool IsArranged
        {
            get { return isArranged; }
        }
        #endregion
        #endregion

        #region Methods
        #region Measure
        /// <summary>
        /// Sets the size of this <see cref="Control"/>.
        /// The parent may or not honor this value.
        /// </summary>
        /// <param name="size">The size to be set.</param>
        public void SetSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        /// <summary>
        /// Sets the size of this <see cref="Control"/>.
        /// The parent may or not honor this value.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Measures this <see cref="Control"/> to determine its desired size.
        /// </summary>
        /// <param name="availableSize">The size available for measurement.</param>
        /// <returns>The desired outer size (margins included) of the <see cref="Control"/>.</returns>
        public Size Measure(Size availableSize)
        {
            if (isMeasured) return cachedDesiredOuterSize;
            if (manager == null) throw new InvalidOperationException("Cannot measure a control without a manager.");

            cachedDesiredOuterSize = MeasureOuterSize(availableSize);

            isMeasured = true;
            return cachedDesiredOuterSize;
        }

        /// <summary>
        /// Measures the desired outer size of this <see cref="Control"/>.
        /// This takes the margin, desired size and minimum size into account.
        /// </summary>
        /// <param name="availableSize">The size available for measurement.</param>
        /// <returns>The desired outer size of this <see cref="Control"/>.</returns>
        protected Size MeasureOuterSize(Size availableSize)
        {
            if (visibilityFlag == Visibility.Collapsed)
            {
                MeasureSize(Size.Zero);
                return Size.Zero;
            }

            int availableWidthWithoutMargins = Math.Max(0, availableSize.Width - margin.TotalX);
            if (width.HasValue && width.Value < availableWidthWithoutMargins)
                availableWidthWithoutMargins = width.Value;

            int availableHeightWithoutMargins = Math.Max(0, availableSize.Height - margin.TotalY);
            if (height.HasValue && height.Value < availableHeightWithoutMargins)
                availableHeightWithoutMargins = height.Value;

            Size desiredSize = MeasureSize(new Size(availableWidthWithoutMargins, availableHeightWithoutMargins));

            return new Size(
                Math.Max(minSize.Width, width.GetValueOrDefault(desiredSize.Width)) + margin.TotalX,
                Math.Max(minSize.Height, height.GetValueOrDefault(desiredSize.Height)) + margin.TotalY);
        }

        /// <summary>
        /// Measures the desired size of this <see cref="Control"/>, excluding its margin.
        /// </summary>
        /// <param name="availableSize">The size available for measurement.</param>
        /// <returns>The desired size of this <see cref="Control"/>.</returns>
        protected abstract Size MeasureSize(Size availableSize);

        /// <summary>
        /// Marks the desired size of this <see cref="Control"/> as dirty.
        /// </summary>
        protected void InvalidateMeasure()
        {
            if (!isMeasured) return;

            isMeasured = false;
            if (parent != null) parent.OnChildMeasureInvalidated(this);
        }

        protected virtual void OnChildMeasureInvalidated(Control child)
        {
            InvalidateMeasure();
            child.InvalidateArrange();
        }
        #endregion

        #region Arrange
        internal void Arrange(Region outerRectangle)
        {
            cachedOuterRectangle = outerRectangle;
            isArranged = true;
        }

        protected abstract void ArrangeChildren();

        protected void DefaultArrangeChild(Control child, Region availableRectangle)
        {
            Region childRectangle = DefaultArrange(availableRectangle, child);
            ArrangeChild(child, childRectangle);
        }

        protected void ArrangeChild(Control child, Region outerRectangle)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            child.cachedOuterRectangle = outerRectangle;
            child.isArranged = true;
            child.ArrangeChildren();
        }

        protected void InvalidateArrange()
        {
            if (!isArranged) return;

            isArranged = false;

            foreach (Control child in Children)
                child.InvalidateArrange();
        }

        public static void DefaultArrange(int availableSize, Alignment alignment, int desiredOuterSize, int maxOuterSize,
            out int min, out int actualOuterSize)
        {
            if (alignment == Alignment.Stretch || desiredOuterSize >= availableSize)
            {
                if (maxOuterSize > availableSize)
                {
                    min = 0;
                    actualOuterSize = availableSize;
                    return;
                }

                desiredOuterSize = maxOuterSize;
                alignment = Alignment.Center;
            }

            actualOuterSize = Math.Min(availableSize, desiredOuterSize);

            switch (alignment)
            {
                case Alignment.Negative:
                    min = 0;
                    break;

                case Alignment.Center:
                    min = availableSize / 2 - desiredOuterSize / 2;
                    break;

                case Alignment.Positive:
                    min = availableSize - actualOuterSize;
                    break;

                default: throw new InvalidEnumArgumentException("alignment", (int)alignment, typeof(Alignment));
            }
        }

        public static Region DefaultArrange(Size availableSpace, Control control)
        {
            Size desiredOuterSize = control.DesiredOuterSize;
            int maxWidth = control.Width.HasValue ? control.Width.Value + control.Margin.TotalX : availableSpace.Width;
            int maxHeight = control.Height.HasValue ? control.Height.Value + control.Margin.TotalY : availableSpace.Height;

            int x, y, width, height;
            DefaultArrange(availableSpace.Width, control.HorizontalAlignment, desiredOuterSize.Width, maxWidth, out x, out width);
            DefaultArrange(availableSpace.Height, control.VerticalAlignment, desiredOuterSize.Height, maxHeight, out y, out height);
            return new Region(x, y, width, height);
        }

        public static Region DefaultArrange(Region availableSpace, Control control)
        {
            Region relativeRectangle = DefaultArrange(availableSpace.Size, control);
            return new Region(
                availableSpace.MinX + relativeRectangle.MinX,
                availableSpace.MinY + relativeRectangle.MinY,
                relativeRectangle.Width, relativeRectangle.Height);
        }
        #endregion
        #endregion
    }
}
