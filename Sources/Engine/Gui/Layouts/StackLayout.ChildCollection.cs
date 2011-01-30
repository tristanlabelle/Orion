using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    partial class StackLayout
    {
        /// <summary>
        /// A collection of a <see cref="StackLayout"/>'s child <see cref="Control"/>s.
        /// </summary>
        public sealed class ChildCollection : Collection<Control>
        {
            #region Fields
            private readonly StackLayout stack;
            #endregion

            #region Constructors
            internal ChildCollection(StackLayout stack)
            {
                this.stack = stack;
            }
            #endregion

            #region Properties
            #endregion

            #region Methods
            protected override void InsertItem(int index, Control item)
            {
                Argument.EnsureNotNull(item, "item");

                stack.AdoptChild(item);
                stack.InvalidateMeasure();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                stack.AbandonChild(Items[index]);
                stack.InvalidateMeasure();
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
