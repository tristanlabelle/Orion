using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    public class DropdownList<T> : Panel
    {
        #region Fields
        private bool enabled = true;

        private IEnumerable<T> items;
        private T selectedItem;
        
        private ListPanel options;
        private Func<T, string> toStringer;
        private Action<Responder, MouseEventArgs> closeDropdownList;
        #endregion

        #region Constructors
        public DropdownList(Rectangle frame)
            : base(frame)
        {
            items = new T[0];
            closeDropdownList = CloseDropdownList;
            toStringer = t => t.ToString();
        }

        public DropdownList(Rectangle frame, IEnumerable<T> items)
            : this(frame)
        {
            Items = items;
        }
        #endregion

        #region Properties
        public IEnumerable<T> Items
        {
            get { return items; }
            set
            {
                items = value;
                SelectedItem = items.First();
            }
        }

        public T SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (!object.Equals(selectedItem, value))
                {
                    selectedItem = value;
                    OnSelectionChanged(selectedItem);
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Func<T, string> ToStringDelegate
        {
            get { return toStringer; }
            set { toStringer = value; }
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
                } while (parent != null);
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
            var handler = SelectionChanged;
            if (handler != null) handler(this, selection);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            base.OnMouseButtonPressed(args);

            if (enabled)
            {
                T[] itemArray = items.ToArray();
                Rectangle listRectangle = Frame.ScaledBy(1, itemArray.Length);
                options = new ListPanel(listRectangle);
                foreach (T item in itemArray)
                    options.Children.Add(CreateRow(item));

                Parent.Children.Add(options);
                TopmostResponder.MouseButtonReleased += closeDropdownList;
            }
            
            return false;
        }

        private Panel CreateRow(T item)
        {
            Panel row = new Panel(Frame);
            row.MouseButtonReleased += (r, args) => SelectedItem = item;
            row.MouseButtonReleased += closeDropdownList;

            Label title = new Label(Frame, toStringer(item));
            row.Children.Add(title);
            return row;
        }

        private void CloseDropdownList(Responder sender, MouseEventArgs args)
        {
            Parent.Children.Remove(options);
            options = null;
            TopmostResponder.MouseButtonReleased -= closeDropdownList;
        }
        #endregion
        #endregion
    }
}
