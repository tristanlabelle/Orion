using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using Orion.Engine.Input;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract partial class Control
    {
        #region Fields
        private UIManager manager;
        private Control parent;
        private IAdornment adornment;
        private Borders margin;
        private Visibility visibilityFlag = Visibility.Visible;
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

        private bool isMouseEventSink;
        private bool hasEnabledFlag = true;
        #endregion

        #region Constructors
        protected Control()
        {
            manager = this as UIManager;
        }
        #endregion

        #region Events
        private HandleableEvent<Control, MouseEvent> mouseMovedEvent;
        private HandleableEvent<Control, MouseEvent> mouseButtonEvent;
        private HandleableEvent<Control, MouseEvent> mouseWheelEvent;
        private HandleableEvent<Control, MouseEvent> mouseClickEvent;
        private HandleableEvent<Control, KeyEvent> keyEvent;
        private HandleableEvent<Control, char> characterTypedEvent;

        /// <summary>
        /// Raised when the mouse moves over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseMoved
        {
            add { mouseMovedEvent.AddHandler(value); }
            remove { mouseMovedEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when a mouse button is pressed or released over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseButton
        {
            add { mouseButtonEvent.AddHandler(value); }
            remove { mouseButtonEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when the mouse wheel is moved over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseWheel
        {
            add { mouseWheelEvent.AddHandler(value); }
            remove { mouseWheelEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when the mouse is clicked over this control or when this control has the mouse capture.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, MouseEvent, bool> MouseClick
        {
            add { mouseClickEvent.AddHandler(value); }
            remove { mouseClickEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when a key event occurs while this control has the keyboard focus.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, KeyEvent, bool> KeyEvent
        {
            add { keyEvent.AddHandler(value); }
            remove { keyEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised when a character is typed while this control has the keyboard focus.
        /// The return value specifies if the event was handled.
        /// </summary>
        public event Func<Control, char, bool> CharacterTyped
        {
            add { characterTypedEvent.AddHandler(value); }
            remove { characterTypedEvent.RemoveHandler(value); }
        }

        /// <summary>
        /// Raised right before this <see cref="Control"/> draws itself.
        /// </summary>
        public event Action<Control, GuiRenderer> PreDrawing;

        internal void RaisePreDrawing()
        {
            PreDrawing.Raise(this, Renderer);
        }

        /// <summary>
        /// Raised right after this <see cref="Control"/> draws itself.
        /// </summary>
        public event Action<Control, GuiRenderer> PostDrawing;

        internal void RaisePostDrawing()
        {
            PostDrawing.Raise(this, Renderer);
        }
        #endregion

        #region Properties
        #region Hierarchy
        /// <summary>
        /// Gets the <see cref="UIManager"/> at the root of this UI hierarchy.
        /// </summary>
        public UIManager Manager
        {
            get { return manager; }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> which contains this <see cref="Control"/> in the UI hierarchy.
        /// </summary>
        public Control Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Enumerates the ancestors of this <see cref="Control"/>.
        /// </summary>
        public IEnumerable<Control> Ancestors
        {
            get
            {
                Control ancestor = parent;
                while (ancestor != null)
                {
                    yield return ancestor;
                    ancestor = ancestor.parent;
                }
            }
        }

        /// <summary>
        /// Enumerates the children of this <see cref="Control"/>.
        /// </summary>
        /// <remarks>
        /// This is implemented through <see cref="GetChildren"/> to allow overriding and shadowing simultaneously in a derived class.
        /// </remarks>
        public IEnumerable<Control> Children
        {
            get { return GetChildren(); }
        }
        #endregion

        /// <summary>
        /// Accesses the <see cref="IAdornment"/> which visually enhances this control.
        /// </summary>
        public IAdornment Adornment
        {
            get { return adornment; }
            set { adornment = value; }
        }

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

        #region Visibility
        /// <summary>
        /// Accesses the current visibility flag of this <see cref="Control"/>.
        /// The actual visibility depends on the flags of this <see cref="Control"/> and its ancestors
        /// </summary>
        public Visibility VisibilityFlag
        {
            get { return visibilityFlag; }
            set
            {
                if (value == visibilityFlag) return;

                Visibility previousVisibilityFlag = visibilityFlag;
                visibilityFlag = value;

                if (visibilityFlag != Visibility.Visible)
                {
                    if (manager != null && HasDescendant(manager.ControlUnderMouse))
                        manager.ControlUnderMouse = Parent;

                    ReleaseKeyboardFocus();
                    ReleaseMouseCapture();
                }

                if (visibilityFlag == Visibility.Collapsed || previousVisibilityFlag == Visibility.Collapsed)
                    InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the actual visibility of this <see cref="Control"/>,
        /// based on this <see cref="Control"/>'s and its parent's <see cref="VisibilityFlag"/>.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                Visibility visibility = Visibility.Visible;

                Control ancestor = this;
                do
                {
                    if (ancestor.VisibilityFlag < visibility)
                    {
                        visibility = ancestor.VisibilityFlag;
                        if (visibility == Visibility.Collapsed) break;
                    }

                    ancestor = ancestor.Parent;
                } while (ancestor != null);

                return visibility;
            }
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

        #region Input & Focus
        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> or one of its descendants are under the mouse cursor.
        /// </summary>
        public bool IsUnderMouse
        {
            get { return manager != null && HasDescendant(manager.ControlUnderMouse); }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> is directly under the mouse cursor.
        /// </summary>
        public bool IsDirectlyUnderMouse
        {
            get { return manager != null && manager.ControlUnderMouse == this; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> currently has the keyboard focus.
        /// </summary>
        public bool HasKeyboardFocus
        {
            get { return manager != null && manager.KeyboardFocusedControl == this; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> currently has captured the mouse.
        /// </summary>
        public bool HasMouseCapture
        {
            get { return manager != null && manager.MouseCapturedControl == this; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> always indicates
        /// mouse events as being handled.
        /// </summary>
        public bool IsMouseEventSink
        {
            get { return isMouseEventSink; }
            set { isMouseEventSink = value; }
        }
        #endregion

        #region Enabled
        /// <summary>
        /// Accesses a value indicating if this <see cref="Control"/> has the enabled flag.
        /// If <c>false</c>, this <see cref="Control"/> and its descendants will be disabled.
        /// </summary>
        public bool HasEnabledFlag
        {
            get { return hasEnabledFlag; }
            set { hasEnabledFlag = value; }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Control"/> is enabled,
        /// taking into account this <see cref="Control"/> and its ancestors' enabled flag.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                Control ancestor = this;
                do
                {
                    if (!hasEnabledFlag) return false;
                    ancestor = ancestor.parent;
                } while (ancestor != null);

                return true;
            }
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="GuiRenderer"/> which draws this <see cref="Control"/>.
        /// </summary>
        protected GuiRenderer Renderer
        {
            get { return manager.Renderer; }
        }
        #endregion

        #region Methods
        #region Hierarchy
        /// <summary>
        /// Obtains the sequence of children of this <see cref="Control"/>.
        /// </summary>
        /// <returns>A sequence of the children of this <see cref="Control"/>.</returns>
        protected virtual IEnumerable<Control> GetChildren()
        {
            return Enumerable.Empty<Control>();
        }

        /// <summary>
        /// Finds a direct child of this <see cref="Control"/> from a point.
        /// </summary>
        /// <param name="point">A point where the child should be, in absolute coordinates.</param>
        /// <returns>The child at that point, or <c>null</c> if no child can be found at that point.</returns>
        public virtual Control GetChildAt(Point point)
        {
        	if (!Rectangle.Contains(point) || Visibility < Visibility.Visible) return null;

            foreach (Control child in Children)
                if (child.VisibilityFlag == Gui2.Visibility.Visible && child.Rectangle.Contains(point))
                    return child;
           
            return null;
        }

        /// <summary>
        /// Gets the deepest descendant <see cref="Control"/> at a given location.
        /// </summary>
        /// <param name="point">The location where to find the descendant.</param>
        /// <returns>The deepest descendant at that location.</returns>
        public Control GetDescendantAt(Point point)
        {
            Control current = this;
            while (true)
            {
                Control descendant = current.GetChildAt(point);
                if (descendant == null) break;
                current = descendant;
            }

            return current;
        }

        /// <summary>
        /// Determines a given <see cref="Control"/> is an ancestor of this <see cref="Control"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be tested.</param>
        /// <returns><c>True</c> if it is this <see cref="Control"/> or one of its ancestors, <c>false</c> if not.</returns>
        public bool HasAncestor(Control control)
        {
            if (control == null) return false;

            Control ancestor = this;
            while (true)
            {
                if (ancestor == control) return true;
                ancestor = ancestor.parent;
                if (ancestor == null) return false;
            }
        }

       /// <summary>
       /// Determines a given <see cref="Control"/> is a descendant of this <see cref="Control"/>.
       /// </summary>
       /// <param name="control">The <see cref="Control"/> to be tested.</param>
       /// <returns><c>True</c> if it is this <see cref="Control"/> or one of its descendants, <c>false</c> if not.</returns>
        public bool HasDescendant(Control control)
        {
            while (true)
            {
                if (control == null) return false;
                if (control == this) return true;
                control = control.Parent;
            }
        }

        /// <summary>
        /// Changes the parent of this <see cref="Control"/> in the UI hierarchy.
        /// </summary>
        /// <param name="parent">The new parent of this <see cref="Control"/>.</param>
        private void SetParent(Control parent)
        {
            if (this is UIManager) throw new InvalidOperationException("The UI manager cannot be a child.");
            if (this.parent != null && parent != null)
            	throw new InvalidOperationException("Cannot set the parent when already parented.");

            isMeasured = false;
            isArranged = false;
            this.parent = parent;
            UIManager newManager = parent == null ? null : parent.manager;
            if (newManager != manager) SetManagerRecursively(newManager);
        }
        
        private void SetManagerRecursively(UIManager manager)
        {
            UIManager previousManager = this.manager;
        	this.manager = manager;
            OnManagerChanged(previousManager);

        	foreach (Control child in Children)
        		child.SetManagerRecursively(manager);
        }

        protected virtual void OnManagerChanged(UIManager previousManager) { }

        protected void AdoptChild(Control child)
        {
            Argument.EnsureNotNull(child, "child");
            if (child.parent == this) return;
            if (child.parent != null) throw new ArgumentException("Cannot add a child which already has another parent.");

            child.SetParent(this);
        }

        protected void AbandonChild(Control child)
        {
            Debug.Assert(child.Parent == this);
            child.SetParent(null);
        }

        /// <summary>
        /// Finds the common ancestor of two <see cref="Control"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// The common ancestor of those <see cref="Control"/>s,
        /// or <c>null</c> if they have no common ancestor or one of them is <c>null</c>.
        /// </returns>
        public static Control FindCommonAncestor(Control a, Control b)
        {
            Control ancestorA = a;
            while (ancestorA != null)
            {
                Control ancestorB = b;
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
                case Alignment.Min:
                    min = 0;
                    break;

                case Alignment.Center:
                    min = availableSize / 2 - desiredOuterSize / 2;
                    break;

                case Alignment.Max:
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

        #region Input Events
        #region Propagation Plumbing
        protected internal bool HandleMouseEvent(MouseEvent @event)
        {
            if (Visibility < Visibility.Visible) return false;

            switch (@event.Type)
            {
                case MouseEventType.Move: return HandleMouseMoved(@event);
                case MouseEventType.Button: return HandleMouseButton(@event);
                case MouseEventType.Wheel: return HandleMouseWheel(@event);
                case MouseEventType.Click: return HandleMouseClick(@event);
                default: throw new InvalidEnumArgumentException("type", (int)@event.Type, typeof(MouseEventType));
            }
        }

        private bool HandleMouseMoved(MouseEvent @event)
        {
            return OnMouseMoved(@event)
                | mouseMovedEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        private bool HandleMouseButton(MouseEvent @event)
        {
            return OnMouseButton(@event)
                | mouseButtonEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        private bool HandleMouseWheel(MouseEvent @event)
        {
            return OnMouseWheel(@event)
                | mouseWheelEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        private bool HandleMouseClick(MouseEvent @event)
        {
            return OnMouseClick(@event)
                | mouseClickEvent.Raise(this, @event)
                | IsMouseEventSink;
        }

        internal bool HandleKeyEvent(KeyEvent @event)
        {
            if (Visibility < Visibility.Visible) return false;

            return OnKeyEvent(@event)
                | keyEvent.Raise(this, @event);
        }

        internal bool HandleCharacterTyped(char character)
        {
            if (Visibility < Visibility.Visible) return false;

            return OnCharacterTyped(character)
                | characterTypedEvent.Raise(this, character);
        }
        #endregion

        #region Overridables
        /// <summary>
        /// When overriden in a derived class, handles a mouse move event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseMoved(MouseEvent @event) { return false; }

        /// <summary>
        /// When overriden in a derived class, handles a mouse button event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseButton(MouseEvent @event) { return false; }

        /// <summary>
        /// When overriden in a derived class, handles a mouse wheel event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseWheel(MouseEvent @event) { return false; }

        /// <summary>
        /// When overriden in a derived class, handles a mouse click event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnMouseClick(MouseEvent @event) { return false; }

        /// <summary>
        /// Gives a chance to this <see cref="Control"/> to handle a key event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnKeyEvent(KeyEvent @event) { return false; }

        /// <summary>
        /// Gives a chance to this <see cref="Control"/>to handle a character event.
        /// </summary>
        /// <param name="event">The event object.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        protected virtual bool OnCharacterTyped(char character) { return false; }

        /// <summary>
        /// Invoked when the mouse cursor enters this <see cref="Control"/>.
        /// </summary>
        protected internal virtual void OnMouseEntered() { }

        /// <summary>
        /// Invoked when the mouse cursor exits this <see cref="Control"/>.
        /// </summary>
        protected internal virtual void OnMouseExited() { }
        #endregion
        #endregion

        #region Focus
        /// <summary>
        /// Gives the keyboard focus to this <see cref="Control"/>.
        /// </summary>
        public void AcquireKeyboardFocus()
        {
            if (manager != null) manager.KeyboardFocusedControl = this;
        }

        /// <summary>
        /// Removes the keyboard focus from this <see cref="Control"/>.
        /// </summary>
        public void ReleaseKeyboardFocus()
        {
            if (HasKeyboardFocus) manager.KeyboardFocusedControl = null;
        }

        /// <summary>
        /// Gives the mouse capture to this <see cref="Control"/>.
        /// </summary>
        public void AcquireMouseCapture()
        {
            if (manager != null) manager.MouseCapturedControl = this;
        }

        /// <summary>
        /// Removes the mouse capture from this <see cref="Control"/>.
        /// </summary>
        public void ReleaseMouseCapture()
        {
            if (HasMouseCapture) manager.MouseCapturedControl = null;
        }

        /// <summary>
        /// Invoked when this <see cref="Control"/> acquires keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses keyboard focus.
        /// </summary>
        protected internal virtual void OnKeyboardFocusLost() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> acquires the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureAcquired() { }

        /// <summary>
        /// Invoked when this <see cref="Control"/> loses the mouse capture.
        /// </summary>
        protected internal virtual void OnMouseCaptureLost() { }
        #endregion

        #region Drawing
        protected internal virtual void Draw() { }
        #endregion
        #endregion
    }
}
