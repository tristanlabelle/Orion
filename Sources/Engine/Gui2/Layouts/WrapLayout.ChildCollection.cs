using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class WrapLayout
    {
        /// <summary>
        /// A collection of a <see cref="WrapLayout"/>'s child <see cref="Control"/>s.
        /// </summary>
        public sealed class ChildCollection : Collection<Control>
        {
            #region Fields
            private readonly WrapLayout wrap;
            #endregion

            #region Constructors
            internal ChildCollection(WrapLayout stack)
            {
                this.wrap = stack;
            }
            #endregion

            #region Properties
            #endregion

            #region Methods
            protected override void InsertItem(int index, Control item)
            {
                Argument.EnsureNotNull(item, "item");

                wrap.AdoptChild(item);
                wrap.InvalidateMeasure();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                wrap.AbandonChild(Items[index]);
                wrap.InvalidateMeasure();
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
