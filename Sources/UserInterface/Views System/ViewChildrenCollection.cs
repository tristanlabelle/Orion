using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Orion.Engine;
using Orion.Engine.Geometry;

namespace Orion.UserInterface
{
    /// <summary>
    /// A collection which manages the children <see cref="View"/>s of a <see cref="View"/>.
    /// </summary>
    /// <remarks>
    /// The z-order of the <see cref="View"/>s within their parent is defined
    /// by their position within this collection.
    /// </remarks>
    [Serializable]
    public sealed class ViewChildrenCollection : Collection<ViewContainer>
    {
        #region Fields
        private readonly ViewContainer parent;
        private readonly List<ViewContainer> children;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the parent container of all this collection's elements. 
        /// </summary>
        public ViewContainer Parent
        {
            get { return parent; }
        }
        #endregion

        #region Constructors
        internal ViewChildrenCollection(ViewContainer parent)
            : base(new List<ViewContainer>())
        {
            Argument.EnsureNotNull(parent, "parent");
            this.parent = parent;
            this.children = (List<ViewContainer>)base.Items;
        }
        #endregion

        #region Methods
        #region Helper Methods
        /// <summary>
        /// Brings a given child <see cref="View"/> to the highest depth.
        /// </summary>
        /// <param name="child">A child <see cref="View"/> to be brought to the front.</param>
        public void BringToFront(ViewContainer child)
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
        public void SendToBack(ViewContainer child)
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
        /// <summary>
        /// Returns an enumerator to pass through the collection. 
        /// </summary>
        /// <returns>
        /// An enumerator for the collection
        /// </returns>
        public new List<ViewContainer>.Enumerator GetEnumerator()
        {
            return children.GetEnumerator();
        }

        /// <summary>
        /// Inserts a view at a specific index. 
        /// </summary>
        /// <param name="index">
        /// A <see cref="System.Int32"/> specifying the index
        /// </param>
        /// <param name="item">
        /// The <see cref="View"/> to insert
        /// </param>
        protected override void InsertItem(int index, ViewContainer item)
        {
            Argument.EnsureNotNull(item, "item");
            if (item is RootView) throw new ArgumentException("The root view cannot be added as a child to another view.");
            if (item.Parent == parent) return;
            if (item.Parent != null) throw new ArgumentException("Expected a view without any parent.");
            if (item.IsAncestorOf(parent))
                throw new ArgumentException("Cannot add an ancestor as a child.");

            base.InsertItem(index, item);
            item.Parent = parent;
            item.OnAddToParent(parent);
            parent.OnChildAdded(item);
        }

        /// <summary>
        /// Removes the view at a specific index.
        /// </summary>
        /// <param name="index">
        /// The index of the view to remove
        /// </param>
        protected override void RemoveItem(int index)
        {
            ViewContainer child = Items[index];
            parent.OnChildRemoved(child);
            child.OnRemovedFromParent(parent);
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        /// <summary>
        /// Clears the list. 
        /// </summary>
        protected override void ClearItems()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                ViewContainer child = Items[i];
                Items[i].Parent = null;
                parent.OnChildRemoved(child);
                child.OnRemovedFromParent(parent);
            }
            base.ClearItems();
        }
        #endregion
        #endregion
    }
}
