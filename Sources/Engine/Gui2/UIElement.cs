using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using System.Diagnostics;
using System.ComponentModel;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract partial class UIElement : IPropertyChangedEventSource
    {
        #region Fields
        private static readonly UIElement[] emptyArray = new UIElement[0];

        private UIManager manager;
        private UIElement parent;
        private Borders margin;
        private Visibility visibility;
        private Alignment horizontalAlignment;
        private Alignment verticalAlignment;
        private Size minimumSize;
        
        /// <summary>
        /// A cached value of the optimal space for this <see cref="UIElement"/> based on the size of its contents.
        /// This value is only meaningful if the layout state is not <see cref="LayoutState.Invalidated"/>.
        /// </summary>
        private Size cachedDesiredReservedSize;
        
        /// <summary>
        /// A cached value of the client space rectangle reserved for this <see cref="UIElement"/>.
        /// This value is only meaningful if the layout state is <see cref="LayoutState.Arranged"/>.
        /// </summary>
        private Region? cachedReservedRectangle;
        
        private LayoutState layoutState;
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
        /// Accesses the minimum size of this <see cref="UIElement"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="UIElement"/>.
        /// </summary>
        public Size MinimumSize
        {
            get { return minimumSize; }
            set
            {
                if (value == minimumSize) return;

                minimumSize = value;
                if (layoutState == LayoutState.Measured
                    && (cachedDesiredReservedSize.Width < minimumSize.Width || cachedDesiredReservedSize.Height < minimumSize.Height))
                {
                    // The cached desired size being smaller than the new minimum size,
                    // the element will have to be measured again so that it's desired size is bigger.
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Accesses the minimum width of this <see cref="UIElement"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="UIElement"/>.
        /// </summary>
        public int MinimumWidth
        {
            get { return minimumSize.Width; }
            set
            {
                Argument.EnsurePositive(value, "MinimumWidth");
                MinimumSize = new Size(value, minimumSize.Height);
            }
        }

        /// <summary>
        /// Accesses the minimum width of this <see cref="UIElement"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="UIElement"/>.
        /// </summary>
        public int MinimumHeight
        {
            get { return minimumSize.Height; }
            set
            {
                Argument.EnsurePositive(value, "MinimumWHeight");
                MinimumSize = new Size(minimumSize.Width, value);
            }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="UIElement"/> can acquire the keyboard focus.
        /// </summary>
        public virtual bool IsFocusable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="UIElement"/> currently has the keyboard focus.
        /// </summary>
        public bool HasFocus
        {
            get { return manager != null && manager.FocusedElement == this; }
        }

        /// <summary>
        /// Gets the collection of children of this <see cref="UIElement"/>.
        /// </summary>
        public ICollection<UIElement> Children
        {
            get { return GetChildren(); }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the value of a property changes.
        /// The parameter specifies the name of the property which value changed.
        /// </summary>
        public event Action<object, string> PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            Argument.EnsureNotNull(propertyName, "propertyName");

            if (PropertyChanged != null) PropertyChanged(this, propertyName);
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
        	if (manager == null) return null;
            if (!GetReservedRectangle().Contains(point)) return null;
            
            return Children.FirstOrDefault(child => child.GetReservedRectangle().Contains(point));
        }

        /// <summary>
        /// Tests if this <see cref="UIElement"/> is an ancestor of a given <see cref="UIElement"/>.
        /// In other words, tests if a given <see cref="UIElement"/> is a descendant of this <see cref="UIElement"/>.
        /// </summary>
        /// <param name="descendant">The descendant to be tested.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="UIElement"/> is an ancestor of <paramref name="descendant"/>,
        /// <c>false</c> if not or if <paramref name="descendant"/> is null.
        /// </returns>
        public bool IsAncestorOf(UIElement descendant)
        {
            while (true)
            {
                if (descendant == null) return false;
                if (descendant == this) return true;
                descendant = descendant.Parent;
            }
        }
        
        /// <summary>
        /// Gets the deepest descendant <see cref="UIElement"/> at a given location.
        /// </summary>
        /// <param name="point">The location where to find the descendant.</param>
        /// <returns>The deepest descendant at that location.</returns>
        public UIElement GetDescendantAt(Point point)
        {
        	if (manager == null) return null;
            if (!GetReservedRectangle().Contains(point)) return null;
            
        	UIElement current = this;
        	while (true)
        	{
        		UIElement descendant = current.GetChildAt(point);
        		if (descendant == null) break;
        		current = descendant;
        	}
        	
        	return current;
        }

        /// <summary>
        /// Changes the parent of this <see cref="UIElement"/> in the UI hierarchy.
        /// </summary>
        /// <param name="parent">The new parent of this <see cref="UIElement"/>.</param>
        private void SetParent(UIElement parent)
        {
            if (this is UIManager) throw new InvalidOperationException("The UI manager cannot be a child.");
            if (this.parent != null && parent != null)
            	throw new InvalidOperationException("Cannot set the parent when already parented.");

            this.parent = parent;
            UIManager newManager = parent == null ? null : parent.manager;
            if (newManager != manager) SetManagerRecursively(newManager);
            layoutState = LayoutState.Invalidated;
        }
        
        private void SetManagerRecursively(UIManager manager)
        {
        	this.manager = manager;
        	foreach (UIElement child in Children)
        		child.SetManagerRecursively(manager);
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
        /// Finds the common ancestor of two <see cref="UIElement"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// The common ancestor of those <see cref="UIElement"/>s,
        /// or <c>null</c> if they have no common ancestor or one of them is <c>null</c>.
        /// </returns>
        public static UIElement FindCommonAncestor(UIElement a, UIElement b)
        {
            UIElement ancestorA = a;
            while (ancestorA != null)
            {
                UIElement ancestorB = b;
                while (ancestorB != null)
                {
                    if (ancestorB == ancestorA) return ancestorA;
                    ancestorB = ancestorB.Parent;
                }
                ancestorA = ancestorA.Parent;
            }
            return null;
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
            if (layoutState == LayoutState.Invalidated)
            {
                Size desiredSize = MeasureWithoutMargin();
                Size clampedDesiredSize = new Size(
                    Math.Max(minimumSize.Width, desiredSize.Width),
                    Math.Max(minimumSize.Height, desiredSize.Height));

                cachedDesiredReservedSize = clampedDesiredSize + margin;

                layoutState = LayoutState.Measured;
            }

            return cachedDesiredReservedSize;
        }

        /// <summary>
        /// Marks the desired size of this <see cref="UIElement"/> as dirty.
        /// </summary>
        protected void InvalidateMeasure()
        {
            if (layoutState == LayoutState.Invalidated) return;

            InvalidateArrange();

        	cachedDesiredReservedSize = Size.Zero;
        	layoutState = LayoutState.Invalidated;

            if (parent != null) parent.OnChildMeasureInvalidated(this);
        }

        protected virtual void OnChildMeasureInvalidated(UIElement child)
        {
            InvalidateMeasure();
        }

        private void AssertUIManagerForMeasurement()
        {
            if (manager == null) throw new InvalidOperationException("Cannot get the measure a UI element without a UI manager.");
        }
        #endregion

        #region Arrange
        protected Region GetReservedRectangle()
        {
            if (layoutState != LayoutState.Arranged)
            {
                if (parent == null)
                    cachedReservedRectangle = new Region(Measure());
                else
                    parent.ArrangeChild(this);

                layoutState = LayoutState.Arranged;
                ArrangeChildren();
            }

            return cachedReservedRectangle.Value;
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
            Region? childrenBounds = GetReservedRectangle() - margin;
            if (!childrenBounds.HasValue) return;

            Region childRectangle = DefaultArrange(childrenBounds.Value.Size, child);
            childRectangle = new Region(childrenBounds.Value.Min + childRectangle.Min, childRectangle.Size);

            SetChildRectangle(child, childRectangle);
        }

        protected void SetChildRectangle(UIElement child, Region rectangle)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);
            Debug.Assert(child.layoutState != LayoutState.Invalidated);

            child.cachedReservedRectangle = rectangle;
            child.layoutState = LayoutState.Arranged;
        }

        protected void InvalidateArrange()
        {
            if (layoutState != LayoutState.Arranged) return;

            cachedReservedRectangle = null;
            layoutState = LayoutState.Measured;

            foreach (UIElement child in Children)
                child.InvalidateArrange();
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
        
        #region Event Handling
        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/> and its ancestors to handle a mouse event.
        /// </summary>
        /// <param name="type">The type of mouse event.</param>
        /// <param name="args">The arguments describing the event.</param>
        /// <returns>The <see cref="UIElement"/> which handled the event, or <c>null</c> if none did.</returns>
        internal UIElement PropagateMouseEvent(MouseEventType type, MouseEventArgs args)
        {
        	UIElement handler = this;
        	do
        	{
        		if (HandleMouseEvent(type, args)) break;
        		handler = handler.parent;
        	} while (handler != null);
        	
        	return handler;
        }
        
        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/> to handle a mouse event.
        /// </summary>
        /// <param name="type">The type of mouse event.</param>
        /// <param name="args">The arguments describing the event.</param>
        /// <returns>
        /// <c>True</c> if the event was handled, <c>false</c> if not.
        /// Returning <c>true</c> stops the propagation of the event through ancestors.
        /// </returns>
        protected virtual bool HandleMouseEvent(MouseEventType type, MouseEventArgs args)
        {
        	return false;
        }

        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/> and its ancestors to handle a key event.
        /// </summary>
        /// <param name="keyAndModifiers">
        /// A <see cref="Keys"/> enumerant containing both the key pressed and the active modifiers.
        /// </param>
        /// <param name="pressed">A value indicating if the key was pressed or released.</param>
        /// <returns>The <see cref="UIElement"/> which handled the event, or <c>null</c> if none did.</returns>
        internal UIElement PropagateKeyEvent(Keys keyAndModifiers, bool pressed)
        {
            UIElement handler = this;
            do
            {
                if (HandleKeyEvent(keyAndModifiers, pressed)) break;
                handler = handler.parent;
            } while (handler != null);

            return handler;
        }

        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/>to handle a key event.
        /// </summary>
        /// <param name="keyAndModifiers">
        /// A <see cref="Keys"/> enumerant containing both the key pressed and the active modifiers.
        /// </param>
        /// <param name="pressed">A value indicating if the key was pressed or released.</param>
        /// <returns>
        /// <c>True</c> if the event was handled, <c>false</c> if not.
        /// Returning <c>true</c> stops the propagation of the event through ancestors.
        /// </returns>
        protected virtual bool HandleKeyEvent(Keys keyAndModifiers, bool pressed)
        {
            return false;
        }

        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/>to handle a character event.
        /// </summary>
        /// <param name="character">The character that was pressed.</param>
        protected internal virtual void HandleCharacterEvent(char character) { }

        /// <summary>
        /// Invoked when the mouse cursor enters this <see cref="UIElement"/>.
        /// </summary>
        protected internal virtual void OnMouseEntered() { }

        /// <summary>
        /// Invoked when the mouse cursor exits this <see cref="UIElement"/>.
        /// </summary>
        protected internal virtual void OnMouseExited() { }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> acquires keyboard focus.
        /// </summary>
        protected internal virtual void OnFocusAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> loses keyboard focus.
        /// </summary>
        protected internal virtual void OnFocusLost() { }
        #endregion

        #region Focus
        /// <summary>
        /// Gives the keyboard focus to this <see cref="UIElement"/>.
        /// </summary>
        public void Focus()
        {
            if (manager != null) manager.FocusedElement = this;
        }

        /// <summary>
        /// Removes the keyboard focus from this <see cref="UIElement"/>.
        /// </summary>
        public void LoseFocus()
        {
            if (HasFocus) manager.FocusedElement = null;
        }
        #endregion

        #region Drawing
        protected void Draw(GraphicsContext graphicsContext)
        {
            if (visibility != Visibility.Visible || GetReservedRectangle().Area == 0) return;

            DoDraw(graphicsContext);
        }

        protected virtual void DoDraw(GraphicsContext graphicsContext)
        {
            DrawChildren(graphicsContext);
        }

        protected void DrawChildren(GraphicsContext graphicsContext)
        {
            Region? childrenAreaBounds = GetReservedRectangle() - margin;
            if (!childrenAreaBounds.HasValue) return;

            DisposableHandle? scissorBoxHandle = null;
            foreach (UIElement child in Children)
            {
                if (!scissorBoxHandle.HasValue)
                {
                    Region childRectangle = child.GetReservedRectangle();
                    if (!childrenAreaBounds.Value.Contains(childRectangle))
                        scissorBoxHandle = graphicsContext.PushScissorRegion(childrenAreaBounds.Value);
                }

                child.Draw(graphicsContext);
            }

            if (scissorBoxHandle.HasValue) scissorBoxHandle.Value.Dispose();
        }
        #endregion
        #endregion
    }
}
