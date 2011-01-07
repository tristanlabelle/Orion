using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Engine.Gui2
{
    partial class OverlapPanel
    {
        public sealed class ChildCollection : Collection<Control>
        {
            #region Fields
            private readonly OverlapPanel panel;
            #endregion

            #region Constructors
            internal ChildCollection(OverlapPanel panel)
            {
                Argument.EnsureNotNull(panel, "panel");

                this.panel = panel;
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
                panel.AbandonChild(item);
                panel.InvalidateMeasure();
            }

            protected override void InsertItem(int index, Control item)
            {
                panel.AdoptChild(item);
                Items.Insert(index, item);
                panel.InvalidateMeasure();
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
