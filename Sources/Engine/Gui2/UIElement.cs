using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using System.Diagnostics;
using System.ComponentModel;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract partial class UIElement
    {
        #region Fields
        private static readonly UIElement[] emptyArray = new UIElement[0];

        private UIManager manager;
        private UIElement parent;
        private Borders margin;
        private Borders padding;
        private Visibility visibility;
        private Alignment horizontalAlignment;
        private Alignment verticalAlignment;
        private Size? cachedMeasuredSize;
        private Region? cachedArrangedRectangle;
        #endregion

        #region Constructors
        protected UIElement()
        {
            manager = this as UIManager;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="UIManager"/> at the root of this UI hierarchy.
        /// </summary>
        public UIManager Manager
        {
            get
            {
                if (manager == null && parent != null) manager = parent.Manager;
                return manager;
            }
        }

        /// <summary>
        /// Gets the <see cref="UIElement"/> which contains this element in the UI hierarchy.
        /// </summary>
        public UIElement Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Accesses the margins around this <see cref="UIElement"/>.
        /// </summary>
        public virtual Borders Margin
        {
            get { return margin; }
            set
            {
                this.margin = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the padding inside this <see cref="UIElement"/>.
        /// </summary>
        public virtual Borders Padding
        {
            get { return padding; }
            set
            {
                this.padding = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the current visibility of this <see cref="UIElement"/>.
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Accesses the horizontal alignment hint for this <see cref="UIElement"/>.
        /// The parent element is charged of honoring or not this value.
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
        /// Accesses the vertical alignment hint for this <see cref="UIElement"/>.
        /// The parent element is charged of honoring or not this value.
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

        /// <summary>
        /// Gets the collection of children of this <see cref="UIElement"/>.
        /// </summary>
        public ICollection<UIElement> Children
        {
            get { return GetChildren(); }
        }
        #endregion

        #region Methods
        #region Hierarchy
        /// <summary>
        /// Obtains the collection of children of this <see cref="UIElement"/>.
        /// </summary>
        /// <returns>The children collection of this <see cref="UIElement"/>.</returns>
        protected virtual ICollection<UIElement> GetChildren()
        {
            return emptyArray;
        }

        /// <summary>
        /// Finds a direct child of this <see cref="UIElement"/> from a point.
        /// </summary>
        /// <param name="point">A point where the child should be, in absolute coordinates.</param>
        /// <returns>The child at that point, or <c>null</c> if no child can be found at that point.</returns>
        public virtual UIElement GetChildAt(Point point)
        {
            if (!Arrange().Contains(point)) return null;
            return Children.FirstOrDefault(child => child.Arrange().Contains(point));
        }

        /// <summary>
        /// Changes the parent of this <see cref="UIElement"/> in the UI hierarchy.
        /// </summary>
        /// <param name="parent">The new parent of this <see cref="UIElement"/>.</param>
        protected virtual void SetParent(UIElement parent)
        {
            if (this is UIManager) throw new InvalidOperationException("The UI manager cannot be a child.");

            this.parent = parent;
            if (parent == null)
            {
                manager = null;
                cachedMeasuredSize = null;
            }

            cachedArrangedRectangle = null;
        }

        protected void AdoptChild(UIElement child)
        {
            child.SetParent(this);
        }

        protected void AbandonChild(UIElement child)
        {
            Debug.Assert(child.Parent == this);
            child.SetParent(null);
        }
        #endregion

        #region Measure
        /// <summary>
        /// Measures the desired size of this <see cref="UIElement"/>, excluding its margin.
        /// </summary>
        /// <returns>The desired size of this <see cref="UIElement"/>.</returns>
        protected abstract Size MeasureWithoutMargin();
        
        /// <summary>
        /// Measures the desired size of this <see cref="UIElement"/>.
        /// </summary>
        /// <returns>The desired size of this <see cref="UIElement"/>.</returns>
        public Size Measure()
        {
            if (!cachedMeasuredSize.HasValue)
            {
                Size sizeWithoutMargin = MeasureWithoutMargin();
                cachedMeasuredSize = new Size(
                    sizeWithoutMargin.Width + Margin.MinX + margin.MaxX,
                    sizeWithoutMargin.Height + Margin.MinY + margin.MaxY);
            }

            return cachedMeasuredSize.Value;
        }

        /// <summary>
        /// Marks the desired size of this <see cref="UIElement"/> as dirty.
        /// </summary>
        protected void InvalidateMeasure()
        {
            cachedMeasuredSize = null;
            cachedArrangedRectangle = null;
            if (parent != null) parent.OnChildMeasureInvalidated(this);
        }

        protected virtual void OnChildMeasureInvalidated(UIElement child) { }
        #endregion

        #region Arrange
        public Region Arrange()
        {
            if (!cachedArrangedRectangle.HasValue)
            {
                if (parent == null)
                    cachedArrangedRectangle = new Region(Measure());
                else
                    parent.ArrangeChild(this);
            }

            return cachedArrangedRectangle.Value;
        }

        protected virtual void ArrangeChildren()
        {
            foreach (UIElement child in Children)
                DefaultArrangeChild(child);
        }

        protected virtual void ArrangeChild(UIElement child)
        {
            ArrangeChildren();
        }

        protected void DefaultArrangeChild(UIElement child)
        {
            Region? childrenBounds = Arrange() - margin - padding;
            if (!childrenBounds.HasValue) return;

            Region childRectangle = DefaultArrange(childrenBounds.Value.Size, child);
            childRectangle = new Region(childrenBounds.Value.Min + childRectangle.Min, childRectangle.Size);

            SetChildRectangle(child, childRectangle);
        }

        protected void SetChildRectangle(UIElement child, Region rectangle)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            child.cachedArrangedRectangle = rectangle;
        }

        protected void InvalidateArrange()
        {
            cachedArrangedRectangle = null;
        }

        public static void DefaultArrange(int availableSize, Alignment alignment, int desiredSize, out int min, out int actualSize)
        {
            if (alignment == Alignment.Stretch || desiredSize >= availableSize)
            {
                min = 0;
                actualSize = availableSize;
                return;
            }

            actualSize = Math.Min(availableSize, desiredSize);

            switch (alignment)
            {
                case Alignment.Min:
                    min = 0;
                    break;

                case Alignment.Center:
                    min = availableSize / 2 - desiredSize / 2;
                    break;

                case Alignment.Max:
                    min = availableSize - actualSize;
                    break;

                default: throw new InvalidEnumArgumentException("alignment", (int)alignment, typeof(Alignment));
            }
        }

        public static Region DefaultArrange(Size availableSpace, UIElement element)
        {
            Size desiredSize = element.Measure();

            int x, y, width, height;
            DefaultArrange(availableSpace.Width, element.HorizontalAlignment, desiredSize.Width, out x, out width);
            DefaultArrange(availableSpace.Height, element.VerticalAlignment, desiredSize.Height, out y, out height);
            return new Region(x, y, width, height);
        }
        #endregion

        protected void Draw(GraphicsContext graphicsContext)
        {
            if (visibility != Visibility.Visible || Arrange().Area == 0) return;

            DoDraw(graphicsContext);
        }

        protected virtual void DoDraw(GraphicsContext graphicsContext)
        {
            DrawChildren(graphicsContext);
        }

        protected void DrawChildren(GraphicsContext graphicsContext)
        {
            Region? childrenAreaBounds = Arrange() - margin - padding;
            if (!childrenAreaBounds.HasValue) return;

            DisposableHandle? scissorBoxHandle = null;
            foreach (UIElement child in Children)
            {
                if (!scissorBoxHandle.HasValue)
                {
                    Region childRectangle = child.Arrange();
                    if (!childrenAreaBounds.Value.Contains(childRectangle))
                        scissorBoxHandle = graphicsContext.PushScissorRegion(childrenAreaBounds.Value);
                }

                child.Draw(graphicsContext);
            }

            if (scissorBoxHandle.HasValue) scissorBoxHandle.Value.Dispose();
        }
        
        private void AssertUIManagerForMeasurement()
        {
            if (manager == null) throw new InvalidOperationException("Cannot get the measure a UI element without a UI manager.");
        }
        #endregion
    }
}
