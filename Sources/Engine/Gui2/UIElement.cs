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
using MouseButtons = System.Windows.Forms.MouseButtons;

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
        private Visibility visibility;
        private Alignment horizontalAlignment;
        private Alignment verticalAlignment;
        private Size minSize;
        
        /// <summary>
        /// A cached value of the optimal space for this <see cref="UIElement"/> based on the size of its contents.
        /// This value is only meaningful if the layout state is not <see cref="LayoutState.Invalidated"/>.
        /// </summary>
        private Size cachedDesiredOuterSize;
        
        /// <summary>
        /// A cached value of the client space rectangle reserved for this <see cref="UIElement"/>.
        /// This value is only meaningful if the layout state is <see cref="LayoutState.Arranged"/>.
        /// </summary>
        private Region? cachedOuterRectangle;
        
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

        #region Margin
        /// <summary>
        /// Accesses the margins around this <see cref="UIElement"/>.
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
        /// Accesses the margin of this <see cref="UIElement"/> on the minimum X-axis side.
        /// </summary>
        public int MinXMargin
        {
            get { return margin.MinX; }
            set { Margin = new Borders(value, margin.MinY, margin.MaxX, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="UIElement"/> on the minimum Y-axis side.
        /// </summary>
        public int MinYMargin
        {
            get { return margin.MinY; }
            set { Margin = new Borders(margin.MinX, value, margin.MaxX, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="UIElement"/> on the maximum X-axis side.
        /// </summary>
        public int MaxXMargin
        {
            get { return margin.MaxX; }
            set { Margin = new Borders(margin.MinX, margin.MinY, value, margin.MaxY); }
        }

        /// <summary>
        /// Accesses the margin of this <see cref="UIElement"/> on the maximum Y-axis side.
        /// </summary>
        public int MaxYMargin
        {
            get { return margin.MaxY; }
            set { Margin = new Borders(margin.MinX, margin.MinY, margin.MaxX, value); }
        }

        /// <summary>
        /// Sets the width of the margin on the left and right of the <see cref="UIElement"/>.
        /// </summary>
        public int XMargin
        {
            set { Margin = new Borders(value, margin.MinY, value, margin.MaxY); }
        }

        /// <summary>
        /// Sets the height of the margin on the top and botton of the <see cref="UIElement"/>.
        /// </summary>
        public int YMargin
        {
            set { Margin = new Borders(margin.MinX, value, margin.MaxX, value); }
        }
        #endregion

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
        public Size MinSize
        {
            get { return minSize; }
            set
            {
                if (value == minSize) return;

                minSize = value;
                if (layoutState == LayoutState.Measured
                    && (cachedDesiredOuterSize.Width < minSize.Width || cachedDesiredOuterSize.Height < minSize.Height))
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
        /// Accesses the minimum width of this <see cref="UIElement"/>, excluding the margins.
        /// This is a hint which can or not be honored by the parent <see cref="UIElement"/>.
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
        /// Gets a value indicating if this <see cref="UIElement"/> currently has the keyboard focus.
        /// </summary>
        public bool HasKeyboardFocus
        {
            get { return manager != null && manager.KeyboardFocusedElement == this; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="UIElement"/> currently has captured the mouse.
        /// </summary>
        public bool HasMouseCapture
        {
            get { return manager != null && manager.MouseCapturedElement == this; }
        }

        /// <summary>
        /// Gets the collection of children of this <see cref="UIElement"/>.
        /// </summary>
        public ICollection<UIElement> Children
        {
            get { return GetChildren(); }
        }

        /// <summary>
        /// Convenience setter to assign initial children to this <see cref="UIElement"/>.
        /// This operation may not be supported by the actual <see cref="UIElement"/> type.
        /// </summary>
        public IEnumerable<UIElement> InitChildren
        {
            set { AddChildren(value); }
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
        /// Adds a child to this <see cref="UIElement"/>.
        /// This is a convenience method, the actual type of this <see cref="UIElement"/> may not support this operation.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to be added.</param>
        public void AddChild(UIElement element)
        {
            Argument.EnsureNotNull(element, "element");
            Children.Add(element);
        }

        /// <summary>
        /// Adds children to this <see cref="UIElement"/>.
        /// This is a convenience method, the actual type of this <see cref="UIElement"/> may not support this operation.
        /// </summary>
        /// <param name="elements">The <see cref="UIElement"/>s to be added.</param>
        public void AddChildren(IEnumerable<UIElement> elements)
        {
            Argument.EnsureNotNull(elements, "elements");

            foreach (UIElement element in elements)
                Children.Add(element);
        }

        /// <summary>
        /// Adds children to this <see cref="UIElement"/>.
        /// This is a convenience method, the actual type of this <see cref="UIElement"/> may not support this operation.
        /// </summary>
        /// <param name="elements">The <see cref="UIElement"/>s to be added.</param>
        public void AddChildren(params UIElement[] elements)
        {
            Argument.EnsureNotNull(elements, "elements");

            foreach (UIElement element in elements)
                Children.Add(element);
        }

        /// <summary>
        /// Finds a direct child of this <see cref="UIElement"/> from a point.
        /// </summary>
        /// <param name="point">A point where the child should be, in absolute coordinates.</param>
        /// <returns>The child at that point, or <c>null</c> if no child can be found at that point.</returns>
        public virtual UIElement GetChildAt(Point point)
        {
        	if (manager == null) return null;

            Region rectangle;
            if (!TryGetRectangle(out rectangle) || !rectangle.Contains(point)) return null;

            foreach (UIElement child in Children)
            {
                Region childRectangle;
                if (child.TryGetRectangle(out childRectangle) && childRectangle.Contains(point))
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Determines a given <see cref="UIElement"/> is an ancestor of this <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to be tested.</param>
        /// <returns><c>True</c> if it is this <see cref="UIElement"/> or one of its ancestors, <c>false</c> if not.</returns>
        public bool HasAncestor(UIElement element)
        {
            if (element == null) return false;

            UIElement ancestor = this;
            while (true)
            {
                if (ancestor == element) return true;
                ancestor = ancestor.parent;
                if (ancestor == null) return false;
            }
        }

       /// <summary>
       /// Determines a given <see cref="UIElement"/> is a descendant of this <see cref="UIElement"/>.
       /// </summary>
       /// <param name="descendant">The <see cref="UIElement"/> to be tested.</param>
       /// <returns><c>True</c> if it is this <see cref="UIElement"/> or one of its descendants, <c>false</c> if not.</returns>
        public bool HasDescendant(UIElement descendant)
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
            Region rectangle;
            if (!TryGetRectangle(out rectangle) || !rectangle.Contains(point)) return null;
            
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
                    Math.Max(minSize.Width, desiredSize.Width),
                    Math.Max(minSize.Height, desiredSize.Height));

                cachedDesiredOuterSize = clampedDesiredSize + margin;

                layoutState = LayoutState.Measured;
            }

            return cachedDesiredOuterSize;
        }

        /// <summary>
        /// Marks the desired size of this <see cref="UIElement"/> as dirty.
        /// </summary>
        protected void InvalidateMeasure()
        {
            if (layoutState == LayoutState.Invalidated) return;

            InvalidateArrange();

        	cachedDesiredOuterSize = Size.Zero;
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
        /// <summary>
        /// Attempts to retreive the outer rectangle of space reserved to this <see cref="UIElement"/>, this value includes the margins.
        /// This operation can fail if this <see cref="UIElement"/> has no manager or if it is completely clipped.
        /// </summary>
        /// <param name="rectangle">
        /// If the operation succeeds, outputs the rectangle of space reserved to this <see cref="UIElement"/>.
        /// </param>
        /// <returns><c>True</c> if the reserved rectangle could be retreived, <c>false</c> if not.</returns>
        public bool TryGetOuterRectangle(out Region rectangle)
        {
            if (manager != null && layoutState != LayoutState.Arranged)
            {
                if (parent == null)
                    cachedOuterRectangle = new Region(Measure());
                else
                    parent.ArrangeChild(this);

                layoutState = LayoutState.Arranged;
                ArrangeChildren();
            }

            return cachedOuterRectangle.TryGetValue(out rectangle);
        }

        /// <summary>
        /// Attempts to retreive the inner rectangle of space reserved to this <see cref="UIElement"/>, this value excludes the margins.
        /// This operation can fail if this <see cref="UIElement"/> has no manager or if it is completely clipped.
        /// </summary>
        /// <param name="rectangle">
        /// If the operation succeeds, outputs the rectangle of space reserved to this <see cref="UIElement"/>.
        /// </param>
        /// <returns><c>True</c> if the reserved rectangle could be retreived, <c>false</c> if not.</returns>
        public bool TryGetRectangle(out Region rectangle)
        {
            Region outerRectangle;
            if (!TryGetOuterRectangle(out outerRectangle))
            {
                rectangle = default(Region);
                return false;
            }

            return Borders.TryShrink(outerRectangle, margin, out rectangle);
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
            Region rectangle;
            if (!TryGetRectangle(out rectangle))
            {
                SetChildOuterRectangle(child, null);
                return;
            }

            Region childRectangle = DefaultArrange(rectangle.Size, child);
            childRectangle = new Region(rectangle.Min + childRectangle.Min, childRectangle.Size);

            SetChildOuterRectangle(child, childRectangle);
        }

        protected void SetChildOuterRectangle(UIElement child, Region? rectangle)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            child.cachedOuterRectangle = rectangle;
            child.layoutState = LayoutState.Arranged;
        }

        protected void InvalidateArrange()
        {
            if (layoutState != LayoutState.Arranged) return;

            cachedOuterRectangle = null;
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
        /// When overriden in a derived class, handles a mouse move event.
        /// </summary>
        /// <param name="state">The current state of the mouse.</param>
        /// <returns><c>True</c> if the event was handled, this stops event propagation. <c>False</c> to let the event propagate.</returns>
        protected internal virtual bool HandleMouseMove(MouseState state)
        {
            return false;
        }

        /// <summary>
        /// When overriden in a derived class, handles a mouse button event.
        /// </summary>
        /// <param name="state">The current state of the mouse.</param>
        /// <param name="button">The involved button.</param>
        /// <param name="pressCount">
        /// The number of successive presses of the button, or <c>0</c> if the button was released.
        /// </param>
        /// <returns><c>True</c> if the event was handled, this stops event propagation. <c>False</c> to let the event propagate.</returns>
        protected internal virtual bool HandleMouseButton(MouseState state, MouseButtons button, int pressCount)
        {
            return false;
        }

        /// <summary>
        /// When overriden in a derived class, handles a mouse wheel event.
        /// </summary>
        /// <param name="state">The current state of the mouse.</param>
        /// <param name="amount">The amount the mouse wheel was moved, in notches.</param>
        /// <returns><c>True</c> if the event was handled, this stops event propagation. <c>False</c> to let the event propagate.</returns>
        protected internal virtual bool HandleMouseWheel(MouseState state, float amount)
        {
            return false;
        }

        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/> to handle a key event.
        /// </summary>
        /// <param name="key">The key that was pressed or released.</param>
        /// <param name="modifiers">The modifier keys which are currently pressed.</param>
        /// <param name="pressed">A value indicating if the key was pressed or released.</param>
        /// <returns>
        /// <c>True</c> if the event was handled, <c>false</c> if not.
        /// Returning <c>true</c> stops the propagation of the event through ancestors.
        /// </returns>
        protected internal virtual bool HandleKey(Keys key, Keys modifiers, bool pressed)
        {
            return false;
        }

        /// <summary>
        /// Gives a chance to this <see cref="UIElement"/>to handle a character event.
        /// </summary>
        /// <param name="character">The character that was pressed.</param>
        protected internal virtual void HandleCharacter(char character) { }

        /// <summary>
        /// Invoked when the mouse cursor enters this <see cref="UIElement"/>.
        /// </summary>
        protected internal virtual void OnMouseEntered() { }

        /// <summary>
        /// Invoked when the mouse cursor exits this <see cref="UIElement"/>.
        /// </summary>
        protected internal virtual void OnMouseExited() { }
        #endregion

        #region Focus
        /// <summary>
        /// Gives the keyboard focus to this <see cref="UIElement"/>.
        /// </summary>
        public void AcquireKeyboardFocus()
        {
            if (manager != null) manager.KeyboardFocusedElement = this;
        }

        /// <summary>
        /// Removes the keyboard focus from this <see cref="UIElement"/>.
        /// </summary>
        public void LoseKeyboardFocus()
        {
            if (HasKeyboardFocus) manager.KeyboardFocusedElement = null;
        }

        /// <summary>
        /// Gives the mouse capture to this <see cref="UIElement"/>.
        /// </summary>
        public void AcquireMouseCapture()
        {
            if (manager != null) manager.MouseCapturedElement = this;
        }

        /// <summary>
        /// Removes the mouse capture from this <see cref="UIElement"/>.
        /// </summary>
        public void LoseMouseCapture()
        {
            if (HasMouseCapture) manager.MouseCapturedElement = null;
        }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> acquires keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> loses keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusLost() { }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> acquires the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="UIElement"/> loses the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureLost() { }
        #endregion
        #endregion
    }
}
