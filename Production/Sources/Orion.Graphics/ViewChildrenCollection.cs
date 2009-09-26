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
        private readonly List<View> children;
        #endregion

        #region Constructors
        internal ViewChildrenCollection(View parent)
            : base(new List<View>())
        {
            Argument.EnsureNotNull(parent, "parent");
            this.parent = parent;
            this.children = (List<View>)base.Items;
        }
        #endregion

        #region Methods
        #region Helper Methods
        /// <summary>
        /// Brings a given child <see cref="View"/> to the highest depth.
        /// </summary>
        /// <param name="child">A child <see cref="View"/> to be brought to the front.</param>
        public void BringToFront(View child)
        {
            Argument.EnsureNotNull(child, "child");
            if (child.Parent != parent)
            {
                throw new ArgumentException(
                    "Expected the view to bring to front to be a children of this view.", "child");
            }

            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] == child)
                {
                    if (i < children.Count - 1)
                    {
                        children.RemoveAt(i);
                        children.Add(child);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Sends a given child <see cref="View"/> to the lowest depth.
        /// </summary>
        /// <param name="child">A child <see cref="View"/> to be sent to the back.</param>
        public void SendToBack(View child)
        {
            Argument.EnsureNotNull(child, "child");
            if (child.Parent != parent)
            {
                throw new ArgumentException(
                    "Expected the view to send back to be a children of this view.", "child");
            }

            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] == child)
                {
                    if (i > 0)
                    {
                        children.RemoveAt(i);
                        children.Insert(0, child);
                    }
                    break;
                }
            }
        }
        #endregion

        #region Overrides & Shadowings
        public new List<View>.Enumerator GetEnumerator()
        {
            return children.GetEnumerator();
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
        #endregion
    }
}
