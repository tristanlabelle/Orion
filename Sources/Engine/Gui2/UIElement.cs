using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract class UIElement
    {
        #region Fields
        private static readonly UIElement[] emptyArray = new UIElement[0];

        private UIManager manager;
        private UIElement parent;
        private Borders margin;
        private UIElementVisibility visibility;
        private Size? cachedDesiredSize;
        private Region? cachedActualRectangle;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="UIManager"/> at the root of this UI hierarchy.
        /// </summary>
        public UIManager Manager
        {
            get { return manager; }
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
        /// Accesses the current visibility of this <see cref="UIElement"/>.
        /// </summary>
        public UIElementVisibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                InvalidateMeasure();
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
            if (!GetActualRectangle().Contains(point)) return null;
            return Children.FirstOrDefault(child => child.GetActualRectangle().Contains(point));
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
                cachedActualRectangle = null;
                cachedDesiredSize = null;
            }
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
            if (!cachedDesiredSize.HasValue) cachedDesiredSize = Measure();

            Size sizeWithoutMargin = MeasureWithoutMargin();
            return new Size(
                sizeWithoutMargin.Width + Margin.MinX + margin.MaxX,
                sizeWithoutMargin.Height + Margin.MinY + margin.MaxY);
        }

        public Region GetActualRectangle()
        {
            if (!cachedActualRectangle.HasValue)
            {
                if (parent == null)
                    cachedActualRectangle = new Region(Measure());
                else
                    parent.ArrangeChild(this);
            }

            return cachedActualRectangle.Value;
        }

        /// <summary>
        /// Marks the desired size of this <see cref="UIElement"/> as dirty.
        /// </summary>
        protected void InvalidateMeasure()
        {
            cachedDesiredSize = null;
            if (parent != null) parent.OnChildMeasureInvalidated(this);
        }

        protected virtual void OnChildMeasureInvalidated(UIElement child) { }

        protected virtual void ArrangeChildren()
        {
            throw new NotImplementedException();
        }

        protected virtual void ArrangeChild(UIElement child)
        {
            ArrangeChildren();
        }

        protected void SetChildRectangle(UIElement child, Region rectangle)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            child.cachedActualRectangle = rectangle;
        }

        protected void Draw(GraphicsContext graphicsContext)
        {
            if (visibility != UIElementVisibility.Visible || GetActualRectangle().Area == 0) return;

            DoDraw(graphicsContext);
        }

        protected virtual void DoDraw(GraphicsContext graphicsContext)
        {
            Region rectangle = GetActualRectangle();

            DisposableHandle? scissorBoxHandle = null;
            foreach (UIElement child in Children)
            {
                if (!scissorBoxHandle.HasValue)
                {
                    Region childRectangle = child.GetActualRectangle();
                    if (!rectangle.Contains(childRectangle))
                        scissorBoxHandle = graphicsContext.PushScissorRegion(rectangle);
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
