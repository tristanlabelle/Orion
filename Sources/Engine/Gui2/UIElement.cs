using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2
{
	/// <summary>
	/// Base class of the UI hierarchy.
	/// </summary>
    public abstract class UIElement
    {
        #region Fields
        private UIManager manager;
        private UIElement parent;
        private Borders margin;
        private UIElementVisibility visibility;
        private Region? cachedRegion;
        private Size? cachedPreferredSize;
        #endregion

        #region Constructors
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

        public virtual Borders Margin
        {
            get { return margin; }
            set
            {
                this.margin = value;
                SetPreferredSizeDirty();
            }
        }

        public UIElementVisibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                SetPreferredSizeDirty();
            }
        }

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
        	return EmptyArray<UIElement>.Rank1;
        }
        
        /// <summary>
        /// Finds a direct child of this <see cref="UIElement"/> from a point.
        /// </summary>
        /// <param name="point">A point where the child should be, in absolute coordinates.</param>
        /// <returns>The child at that point, or <c>null</c> if no child can be found at that point.</returns>
        public virtual UIElement GetChildAt(Point point)
        {
        	return Children.FirstOrDefault(child => child.GetRegion().Contains(point));
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
            	cachedRegion = null;
            	cachedPreferredSize = null;
            }
        }

        /// <summary>
        /// Measures the preferred size of this <see cref="UIElement"/>, excluding its margin.
        /// </summary>
        /// <returns>The preferred size of this <see cref="UIElement"/>.</returns>
        protected virtual Size MeasurePreferredSizeWithoutMargin()
        {
        	throw new NotImplementedException();
        }
        
        /// <summary>
        /// Measures the preferred size of this <see cref="UIElement"/>.
        /// </summary>
        /// <returns>The preferred size of this <see cref="UIElement"/>.</returns>
        protected virtual Size MeasurePreferredSize()
        {
            Size sizeWithoutMargin = MeasurePreferredSizeWithoutMargin();
            return new Size(
                sizeWithoutMargin.Width + Margin.MinX + margin.MaxX,
                sizeWithoutMargin.Height + Margin.MinY + margin.MaxY);
        }

        /// <summary>
        /// Obtains the preferred size of this <see cref="UIElement"/>,
        /// measuring it if needed.
        /// This method should only be called once a <see cref="UIManager"/> is set.
        /// </summary>
        /// <returns>The preferred size of the <see cref="UIElement"/>.</returns>
        protected Size GetPreferredSize()
        {
        	AssertUIManagerForMeasurement();
        	if (!cachedPreferredSize.HasValue) cachedPreferredSize = MeasurePreferredSize();
        	return cachedPreferredSize.Value;
        }
        
        protected Region GetRegion()
        {
        	AssertUIManagerForMeasurement();
        	if (!cachedRegion.HasValue)
        	{
        		if (parent == null)
        			cachedRegion = new Region(GetPreferredSize());
        		else
        			cachedRegion = parent.MeasureChildRegion(this);
        	}
        	
        	return cachedRegion.Value;
        }

        /// <summary>
        /// Sets the preferred size of this <see cref="UIElement"/> as dirty.
        /// </summary>
        protected void SetPreferredSizeDirty()
        {
        	cachedPreferredSize = null;
        	if (parent != null) parent.OnChildPreferredSizeSetDirty(this);
        }
        
        protected virtual void OnChildPreferredSizeSetDirty(UIElement child) { }
        
        protected virtual Region MeasureChildRegion(UIElement child)
        {
        	throw new NotImplementedException();
        }

        protected void Draw(GraphicsContext graphicsContext)
        {
        	if (visibility != UIElementVisibility.Visible || GetRegion().Area == 0) return;
        	DoDraw(graphicsContext);
        }
        
        protected virtual void DoDraw(GraphicsContext graphicsContext)
        {
        	DrawChildren(graphicsContext);
        }
        
        protected void DrawChildren(GraphicsContext graphicsContext)
        {
        	foreach (UIElement child in Children)
        		child.Draw(graphicsContext);
        }
        
        private void AssertUIManagerForMeasurement()
        {
        	if (manager == null) throw new InvalidOperationException("Cannot get the measure a UI element without a UI manager.");
        }
        #endregion
    }
}
