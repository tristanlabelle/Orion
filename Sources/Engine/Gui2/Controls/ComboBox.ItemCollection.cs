using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui2
{
    partial class ComboBox
    {
        /// <summary>
        /// The collection of items in a <see cref="ComboBox"/>.
        /// </summary>
        public sealed class ItemCollection : Collection<Control>
        {
            #region Fields
            private readonly ComboBox comboBox;
            #endregion

            #region Constructors
            internal ItemCollection(ComboBox comboBox)
                : base(comboBox.dropDownList.Items)
            {
                Argument.EnsureNotNull(comboBox, "comboBox");

                this.comboBox = comboBox;
            }
            #endregion

            #region Methods
            protected override void InsertItem(int index, Control item)
            {
                // Do not AdoptChild, as the DropDownList's StackLayout will do so.
                base.InsertItem(index, item);
                if (Count == 1) comboBox.SelectedItem = item;
            }

            protected override void RemoveItem(int index)
            {
                Control item = Items[index];
                // Do not AbandonChild, as the DropDownList's StackLayout will do so.
                base.RemoveItem(index);
                if (comboBox.SelectedItem == item)
                    comboBox.SelectedItemIndex = Math.Min(index, Count - 1);
            }

            protected override void SetItem(int index, Control item)
            {
                // Do not AdoptChild or AbandonChild, as the DropDownList's StackLayout will do so.
                base.SetItem(index, item);
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }
            #endregion
        }
    }
}
