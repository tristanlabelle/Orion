using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class StackPanel
    {
        public sealed class ChildCollection : Collection<Control>
        {
            #region Fields
            private readonly StackPanel stackPanel;
            #endregion

            #region Constructors
            internal ChildCollection(StackPanel stackPanel)
            {
                this.stackPanel = stackPanel;
            }
            #endregion

            #region Properties
            #endregion

            #region Methods
            protected override void InsertItem(int index, Control item)
            {
                Argument.EnsureNotNull(item, "item");

                stackPanel.AdoptChild(item);
                stackPanel.InvalidateMeasure();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                stackPanel.AbandonChild(Items[index]);
                stackPanel.InvalidateMeasure();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, Control item)
            {
                RemoveItem(index);
                InsertItem(index, item);
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }
            #endregion
        }
    }
}
