﻿using System;
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
        private IEnumerable<T> items;
        private T selectedItem;
        private ListPanel options;
        private Func<T, string> toStringer;
        #endregion

        #region Constructors
        public DropdownList(Rectangle frame)
            : base(frame)
        {
            items = new T[0];
        }

        public DropdownList(Rectangle frame, IEnumerable<T> items)
            : this(frame)
        {
            this.items = items;
        }
        #endregion

        #region Properties
        public IEnumerable<T> Items
        {
            get { return items; }
            set { items = value; }
        }

        public T SelectedItem
        {
            get { return selectedItem; }
            set
            {
                Argument.EnsureIn(value, items, "value");
                selectedItem = value;
                OnSelectionChanged(selectedItem);
            }
        }

        public Func<T, string> ToStringDelegate
        {
            get { return toStringer; }
            set { toStringer = value; }
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

            T[] itemArray = items.ToArray();
            Rectangle listRectangle = Frame.ScaledBy(1, itemArray.Length);
            options = new ListPanel(listRectangle);
            foreach (T item in itemArray)
            {
                Panel row = new Panel(Frame);
                Label title = new Label(Frame, toStringer(item));
                row.Children.Add(title);
            }
            
            return false;
        }
        #endregion
        #endregion
    }
}
