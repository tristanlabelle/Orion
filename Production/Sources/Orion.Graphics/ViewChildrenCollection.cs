using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.Graphics
{
    /// <summary>
    /// A collection which manages the children <see cref="View"/>s of a <see cref="View"/>.
    /// </summary>
    /// <remarks>
    /// The z-order of the <see cref="View"/>s within their parent is defined
    /// by their position within this collection.
    /// </remarks>
    [Serializable]
    public sealed class ViewChildrenCollection : Collection<View>
    {
        #region Fields
        private readonly View parent;
        #endregion

        #region Constructors
        internal ViewChildrenCollection(View parent)
        {
            Argument.EnsureNotNull(parent, "parent");
            this.parent = parent;
        }
        #endregion

        #region Methods
        public new List<View>.Enumerator GetEnumerator()
        {
            return ((List<View>)Items).GetEnumerator();
        }

        protected override void InsertItem(int index, View item)
        {
            Argument.EnsureNotNull(item, "item");
            if (item.Parent != null) throw new ArgumentException("Expected a view without any parent.");
            if (Contains(item)) return;
            if (item.IsAncestorOf(parent))
                throw new ArgumentException("Cannot add an ancestor as a child.");

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            for (int i = 0; i < Items.Count; ++i)
                Items[i].Parent = null;
            base.ClearItems();
        }
        #endregion
    }
}
