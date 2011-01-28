using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A control which provides a list of items from which the user can make a selection.
    /// </summary>
    public partial class ListBox : Control
    {
        #region Fields
        private readonly StackLayout itemStack;
        private readonly ItemCollection items;
        private int selectedItemIndex = -1;
        private int highlightedItemIndex = -1;
        private ColorRgba selectionColor = Colors.TransparentBlack;
        private ColorRgba highlightColor = Colors.TransparentBlack;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ListBox"/>.
        /// </summary>
        public ListBox()
        {
            this.itemStack = new StackLayout()
            {
                Direction = Direction.PositiveY
            };
            AdoptChild(this.itemStack);

            this.items = new ItemCollection(this, itemStack);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the selected item changes.
        /// </summary>
        public event Action<ListBox> SelectionChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="StackLayout"/> which provides the layout for the items.
        /// </summary>
        /// <remarks>
        /// Do not modify this object other than visually.
        /// </remarks>
        public StackLayout ItemStack
        {
            get { return itemStack; }
        }

        /// <summary>
        /// Gets the collection of the items displayed by this <see cref="ListBox"/>.
        /// </summary>
        public ItemCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Gets the gap, in pixels, between successive items.
        /// </summary>
        public int ItemGap
        {
            get { return itemStack.ChildGap; }
            set { itemStack.ChildGap = value; }
        }

        /// <summary>
        /// Accesses the padding between this <see cref="ListBox"/> and its items.
        /// </summary>
        /// <remarks>
        /// <see cref="ListBox"/>es have no padding per se,
        /// this property changes the margin of its item stack.
        /// </remarks>
        public Borders Padding
        {
            get { return itemStack.Margin; }
            set { itemStack.Margin = value; }
        }

        /// <summary>
        /// Gets the index of the item that is currently selected.
        /// This is <c>-1</c> if no item is selected.
        /// </summary>
        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                if (value == selectedItemIndex) return;
                if (value < -1 || value >= items.Count) throw new ArgumentOutOfRangeException("SelectedItemIndex");
                
                selectedItemIndex = value;
                if (highlightedItemIndex == -1) highlightedItemIndex = value;
                SelectionChanged.Raise(this);
            }
        }

        /// <summary>
        /// Gets the item that is currently selected.
        /// This is <c>null</c> if no item is selected.
        /// </summary>
        public Control SelectedItem
        {
            get { return selectedItemIndex == -1 ? null : items[selectedItemIndex]; }
            set { SelectedItemIndex = items.IndexOf(value); }
        }

        /// <summary>
        /// Accesses the color of the highlighting of the selected item.
        /// </summary>
        public ColorRgba SelectionColor
        {
            get { return selectionColor; }
            set { selectionColor = value; }
        }

        /// <summary>
        /// Gets the index of the item that is currently highlighted.
        /// This is <c>-1</c> if no item is highlighted.
        /// </summary>
        public int HighlightedItemIndex
        {
            get { return highlightedItemIndex; }
            set
            {
                if (value == highlightedItemIndex) return;
                if (value < -1 || value >= items.Count) throw new ArgumentOutOfRangeException("HighlightedItemIndex");

                highlightedItemIndex = value;
            }
        }

        /// <summary>
        /// Gets the item that is currently highlighted.
        /// This is <c>null</c> if no item is highlighted.
        /// </summary>
        public Control HighlightedItem
        {
            get { return highlightedItemIndex == -1 ? null : items[highlightedItemIndex]; }
            set { HighlightedItemIndex = items.IndexOf(value); }
        }

        /// <summary>
        /// Accesses the color of the highlighting of the item under the mouse.
        /// </summary>
        public ColorRgba HighlightColor
        {
            get { return highlightColor; }
            set { highlightColor = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an item to this <see cref="ListBox"/>.
        /// </summary>
        /// <param name="control">The control to be added.</param>
        public void AddItem(Control control)
        {
        	items.Add(control);
        }
        
        protected override IEnumerable<Control> GetChildren()
        {
            yield return itemStack;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            return itemStack.Measure(availableSize);
        }

        protected override void ArrangeChildren()
        {
            ArrangeChild(itemStack, Rectangle);
        }

        protected override bool OnMouseMoved(MouseEvent @event)
        {
            if (!Rectangle.Contains(@event.Position))
            {
                highlightedItemIndex = -1;
                return true;
            }

            // Highlight the item that is the closest to the cursor, the cheap way.
            float closestSquaredDistance = float.PositiveInfinity;
            for (int i = 0; i < itemStack.Children.Count; ++i)
            {
                Control item = itemStack.Children[i];
                float squaredDistance = ((Vector2)(@event.Position - item.Rectangle.Clamp(@event.Position))).LengthSquared;
                if (squaredDistance < closestSquaredDistance)
                {
                    highlightedItemIndex = i;
                    closestSquaredDistance = squaredDistance;
                }
            }

            return true;
        }

        protected override bool OnMouseButton(MouseEvent @event)
        {
            if (highlightedItemIndex != -1)
                SelectedItemIndex = highlightedItemIndex;

            return true;
        }

        protected internal override void Draw()
        {
            if (highlightedItemIndex == -1)
            {
                Debug.Assert(selectedItemIndex == -1,
                    "There should always be a highlighted item when there is a selected one.");
                return;
            }

            if (selectedItemIndex != -1)
                DrawHighlight(selectedItemIndex, selectionColor);

            if (highlightedItemIndex != selectedItemIndex)
                DrawHighlight(highlightedItemIndex, highlightColor);
        }

        private void DrawHighlight(int index, ColorRgba color)
        {
            Region rectangle = itemStack.Rectangle;
            Region itemOuterRectangle = items[index].OuterRectangle;

            Region highlightRectangle = new Region(
                rectangle.MinX, itemOuterRectangle.MinY,
                rectangle.Width, itemOuterRectangle.Height);

            Renderer.DrawRectangle(highlightRectangle, color);
        }
        #endregion
    }
}
