using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using OpenTK.Math;

namespace Orion.Engine.Gui
{
    public class DropdownList<T> : Panel
    {
        #region Fields
        private bool enabled = true;

        private IEnumerable<T> items;
        private T selectedItem;
        private bool isItemSelected;
        private Func<T, string> stringConverter;

        private readonly Label shownTitleLabel;
        private ListPanel optionsListPanel;

        private readonly Action<Responder, MouseEventArgs> rootMouseButtonReleasedEventHandler;
        #endregion

        #region Constructors
        public DropdownList(Rectangle frame)
            : base(frame)
        {
            this.items = Enumerable.Empty<T>();
            this.stringConverter = t => t.ToString();
            this.shownTitleLabel = new Label(new Rectangle(frame.Size), string.Empty);
            this.rootMouseButtonReleasedEventHandler = (s, a) => Close();

            Children.Add(this.shownTitleLabel);
        }

        public DropdownList(Rectangle frame, IEnumerable<T> items)
            : this(frame)
        {
            this.Items = items;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the sequence of items used as a source of elements for this list.
        /// </summary>
        public IEnumerable<T> Items
        {
            get { return items; }
            set
            {
                items = value;
                SelectedItem = items.First();
            }
        }

        /// <summary>
        /// Gets a value indicating if an item is currently selected.
        /// </summary>
        public bool IsItemSelected
        {
            get { return isItemSelected; }
        }

        /// <summary>
        /// Gets the currently selected item if there's one.
        /// </summary>
        public T SelectedItem
        {
            get
            {
                if (!isItemSelected) throw new InvalidOperationException("Cannot get the selected item when none is.");
                return selectedItem;
            }
            set
            {
                if (IsItemSelected && EqualityComparer<T>.Default.Equals(selectedItem, value))
                    return;
                
                selectedItem = value;
                isItemSelected = true;
                OnSelectionChanged(selectedItem);
            }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="DropdownList{T}"/>
        /// can be manipulated by the user.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Accesses the delegate to the method used to convert elements of this list
        /// to their string representation.
        /// </summary>
        public Func<T, string> StringConverter
        {
            get { return stringConverter; }
            set
            {
                Argument.EnsureNotNull(value, "StringConverter");
                stringConverter = value;
                shownTitleLabel.Text = value(selectedItem);
            }
        }

        private Responder TopmostResponder
        {
            get
            {
                Responder parent = this;
                Responder checkedParent;
                do
                {
                    checkedParent = parent.Parent as Responder;
                    if (checkedParent != null)
                        parent = checkedParent;
                } while (checkedParent != null);
                return parent;
            }
        }
        #endregion

        #region Events
        public event Action<DropdownList<T>, T> SelectionChanged;
        #endregion

        #region Methods
        #region Event Handling
        private void OnSelectionChanged(T selection)
        {
            if (shownTitleLabel != null)
                Children.Remove(shownTitleLabel);

            shownTitleLabel.Text = stringConverter(selectedItem);
            Children.Add(shownTitleLabel);

            var handler = SelectionChanged;
            if (handler != null) handler(this, selection);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            base.OnMouseButtonPressed(args);

            if (enabled)
            {
                T[] itemArray = items.ToArray();
                Rectangle listRectangle = Frame;
                Vector2 top = new Vector2(listRectangle.MinX, listRectangle.MaxY);
                listRectangle = listRectangle.TranslatedTo(0, 0).ScaledBy(1, itemArray.Length);
                listRectangle = listRectangle.TranslatedTo(top.X, top.Y - listRectangle.Height);
                listRectangle = ConvertToTopmostCoordinatesSystem(listRectangle);
                optionsListPanel = new ListPanel(listRectangle);
                foreach (T item in itemArray)
                    optionsListPanel.Children.Add(CreateRow(item));

                TopmostResponder.Children.Add(optionsListPanel);
                TopmostResponder.MouseButtonReleased += rootMouseButtonReleasedEventHandler;
            }
            
            return false;
        }

        private void Close()
        {
            TopmostResponder.Children.Remove(optionsListPanel);
            optionsListPanel = null;
            TopmostResponder.MouseButtonReleased -= rootMouseButtonReleasedEventHandler;
        }

        private Panel CreateRow(T item)
        {
            Panel row = new Panel(Frame);
            row.MouseButtonReleased += (r, args) => { SelectedItem = item; Close(); };

            Label title = new Label(stringConverter(item));
            row.Children.Add(title);
            return row;
        }


        private Rectangle ConvertToTopmostCoordinatesSystem(Rectangle rect)
        {
            Responder parent = this.Parent as Responder;
            while (parent != null)
            {
                Vector2 scaleBy = new Vector2(parent.Bounds.Width / parent.Frame.Width, parent.Bounds.Height / parent.Frame.Height);
                rect = rect.TranslatedBy(parent.Frame.Min).ScaledBy(scaleBy).TranslatedBy(-parent.Bounds.Min);
                parent = parent.Parent as Responder;
            }
            return rect;
        }
        #endregion
        #endregion
    }
}
