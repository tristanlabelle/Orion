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
        private readonly DropDownPopup dropDown;
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

            dropDown = new DropDownPopup(this);
            dropDown.VisibilityFlag = Visibility.Hidden;
            dropDown.ListBox.SelectionChanged += OnSelectionChanged;
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
        /// Gets the <see cref="DropDownPopup"/> displayed when this <see cref="ComboBox"/> is open.
        /// </summary>
        public DropDownPopup DropDown
        {
            get { return dropDown; }
        }

        /// <summary>
        /// Gets the <see cref="ListBox"/> within the drop-down popup.
        /// </summary>
        public ListBox ListBox
        {
            get { return dropDown.ListBox; }
        }

        /// <summary>
        /// Gets the collection of items that are displayed by this <see cref="ComboBox"/>.
        /// </summary>
        public IList<Control> Items
        {
            get { return ListBox.Items; }
        }

        /// <summary>
        /// Accesses the <see cref="Control"/> that is currently selected.
        /// </summary>
        public Control SelectedItem
        {
            get { return ListBox.SelectedItem; }
            set { ListBox.SelectedItem = value; }
        }

        /// <summary>
        /// Accesses the index of the currently selected item.
        /// </summary>
        public int SelectedItemIndex
        {
            get { return ListBox.SelectedItemIndex; }
            set { ListBox.SelectedItemIndex = value; }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="ComboBox"/>'s drop down is currently visible.
        /// </summary>
        public bool IsOpen
        {
            get { return dropDown.VisibilityFlag == Visibility.Visible; }
            set { dropDown.VisibilityFlag = value ? Visibility.Visible : Visibility.Hidden; }
        }
        #endregion

        #region Methods
        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Popups.Remove(dropDown);
            if (Manager != null) Manager.Popups.Add(dropDown);
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

        private void OnSelectionChanged(ListBox sender)
        {
            selectedItemViewport.ViewedControl = sender.SelectedItem;
            dropDown.VisibilityFlag = Visibility.Hidden;
        }

        private void OnButtonClicked(Button sender, ButtonClickEvent @event)
        {
            dropDown.VisibilityFlag = Visibility.Visible;
            dropDown.AcquireMouseCapture();
            dropDown.AcquireKeyboardFocus();
        }
        #endregion
    }
}
