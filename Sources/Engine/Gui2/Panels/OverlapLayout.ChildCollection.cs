using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui2
{
    partial class OverlapLayout
    {
        /// <summary>
        /// A collection of <see cref="OverlapLayout"/> child <see cref="Control"/>s.
        /// </summary>
        public sealed class ChildCollection : Collection<Control>
        {
            #region Fields
            private readonly OverlapLayout layout;
            #endregion

            #region Constructors
            internal ChildCollection(OverlapLayout layout)
            {
                Argument.EnsureNotNull(layout, "layout");

                this.layout = layout;
            }
            #endregion

            #region Methods
            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }

            protected override void RemoveItem(int index)
            {
                Control item = Items[index];
                Items.RemoveAt(index);
                layout.AbandonChild(item);
                layout.InvalidateMeasure();
            }

            protected override void InsertItem(int index, Control item)
            {
                layout.AdoptChild(item);
                Items.Insert(index, item);
                layout.InvalidateMeasure();
            }

            protected override void SetItem(int index, Control item)
            {
                RemoveItem(index);
                InsertItem(index, item);
            }
            #endregion
        }
    }
}
