using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui
{
    partial class ListBox
    {
        /// <summary>
        /// The collection of items in a <see cref="ListBox"/>.
        /// </summary>
        public sealed class ItemCollection : Collection<Control>
        {
            #region Fields
            private readonly ListBox listBox;
            #endregion

            #region Constructors
            internal ItemCollection(ListBox listBox, StackLayout stack)
                : base(stack.Children)
            {
                Argument.EnsureNotNull(listBox, "listBox");

                this.listBox = listBox;
            }
            #endregion

            #region Methods
            protected override void InsertItem(int index, Control item)
            {
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);

                if (listBox.selectedItemIndex == index)
                    listBox.selectedItemIndex = Math.Min(index, Count - 1);

                if (listBox.highlightedItemIndex == index)
                    listBox.highlightedItemIndex = listBox.selectedItemIndex;
            }

            protected override void SetItem(int index, Control item)
            {
                if (this[index] == item) return;

                base.SetItem(index, item);
                listBox.SelectionChanged.Raise(listBox);
            }

            protected override void ClearItems()
            {
                bool selectionChanged = listBox.selectedItemIndex != -1;

                listBox.highlightedItemIndex = -1;
                listBox.selectedItemIndex = -1;
                while (Count > 0) base.ClearItems();

                if (selectionChanged) listBox.SelectionChanged.Raise(listBox);
            }
            #endregion
        }
    }
}
