using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A control which enables the user to select a single element by means of a drop-down list.
    /// </summary>
    public partial class ComboBox : Control
    {
        #region Fields
        private readonly DockLayout dock;
        private readonly Button button;
        private readonly ControlViewport selectedItemViewport;
        private readonly DropDownList dropDownList;
        private readonly ItemCollection items;
        #endregion

        #region Constructors
        public ComboBox()
        {
            dock = new DockLayout() { LastChildFill = true };
            AdoptChild(dock);

            button = new Button();
            button.Clicked += OnButtonClicked;
            dock.Dock(button, Direction.PositiveX);

            selectedItemViewport = new ControlViewport();
            dock.Dock(selectedItemViewport, Direction.NegativeX);

            dropDownList = new DropDownList(this);
            dropDownList.VisibilityFlag = Visibility.Hidden;

            items = new ItemCollection(this); // Must be created after the DropDownList
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the item that is currently selected changes.
        /// </summary>
        public event Action<ComboBox> SelectedItemChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Button"/> which opens this <see cref="ComboBox"/>.
        /// </summary>
        public Button Button
        {
            get { return button; }
        }

        /// <summary>
        /// Gets the <see cref="ControlViewport"/> which displays the currently selected item.
        /// </summary>
        public ControlViewport SelectedItemViewport
        {
            get { return selectedItemViewport; }
        }

        /// <summary>
        /// Gets the <see cref="DropDownList"/> displayed when this <see cref="ComboBox"/> is open.
        /// </summary>
        public DropDownList DropDown
        {
            get { return dropDownList; }
        }

        /// <summary>
        /// Gets the collection of items that are displayed by this <see cref="ComboBox"/>.
        /// </summary>
        public ItemCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> that is currently selected.
        /// </summary>
        public Control SelectedItem
        {
            get { return selectedItemViewport.ViewedControl; }
            set
            {
                if (value == SelectedItem) return;

                selectedItemViewport.ViewedControl = value;
                SelectedItemChanged.Raise(this);
            }
        }

        /// <summary>
        /// Accesses the index of the currently selected item.
        /// </summary>
        public int SelectedItemIndex
        {
            get { return Items.IndexOf(SelectedItem); }
            set { SelectedItem = value < 0 ? null : Items[value]; }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="ComboBox"/>'s drop down is currently visible.
        /// </summary>
        public bool IsOpen
        {
            get { return dropDownList.VisibilityFlag == Visibility.Visible; }
            set { dropDownList.VisibilityFlag = value ? Visibility.Visible : Visibility.Hidden; }
        }
        #endregion

        #region Methods
        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Popups.Remove(dropDownList);
            if (Manager != null) Manager.Popups.Add(dropDownList);
        }

        protected override IEnumerable<Control> GetChildren()
        {
            yield return dock;
        }

        protected override Size MeasureSize(Size availableSize)
        {
            return dock.Measure(availableSize);
        }

        protected override void ArrangeChildren()
        {
            DefaultArrangeChild(dock, Rectangle);
        }

        private void OnButtonClicked(Button sender, ButtonClickEvent @event)
        {
            dropDownList.VisibilityFlag = Visibility.Visible;
            dropDownList.AcquireMouseCapture();
            dropDownList.AcquireKeyboardFocus();
        }
        #endregion
    }
}
