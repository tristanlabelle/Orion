using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Orion.Geometry;

namespace Orion.UserInterface
{
    public abstract class ViewContainer : IDisposable
    {
        #region Nested Types
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
            private ViewContainer parent;
            private readonly List<ViewContainer> children;
            #endregion

            #region Properties
            /// <summary>
            /// Accesses the parent container of all this collection's elements. 
            /// </summary>
            public ViewContainer Parent
            {
                get { return parent; }
                internal set { parent = value; }
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
                if (item.Parent != null) throw new ArgumentException("Expected a view without any parent.");
                if (Contains(item)) return;
                if (item.IsAncestorOf(parent))
                    throw new ArgumentException("Cannot add an ancestor as a child.");

                base.InsertItem(index, item);
                item.Parent = parent;
                item.OnAddToParent(parent);
                parent.OnAddChild(item);
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
                parent.OnRemoveChild(child);
                child.OnRemoveFromParent(parent);
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
                    parent.OnRemoveChild(child);
                    child.OnRemoveFromParent(parent);
                }
                base.ClearItems();
            }
            #endregion
            #endregion
        }
        #endregion

        #region Fields
        private Collection<ViewContainer> children;
        private ViewContainer parent;
        protected internal bool isDisposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a ViewContainer.
        /// </summary>
        public ViewContainer()
        {
            children = new ViewChildrenCollection(this);
        }

        public ViewContainer(Collection<ViewContainer> childrenCollection)
        {
            children = childrenCollection;
        }
        #endregion

        #region Events

        public event GenericEventHandler<ViewContainer, ViewContainer> AddedToParent;
        public event GenericEventHandler<ViewContainer, ViewContainer> RemovedFromParent;
        public event GenericEventHandler<ViewContainer, ViewContainer> AddedChild;
        public event GenericEventHandler<ViewContainer, ViewContainer> RemovedChild;
        public event GenericEventHandler<ViewContainer, ViewContainer> AncestryChanged;

        #endregion

        #region Properties
        public virtual Rectangle Frame { get; set; }
        public virtual Rectangle Bounds { get; set; }

        public IEnumerable<ViewContainer> Ancestors
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                ViewContainer ancestor = Parent;
                while (ancestor != null)
                {
                    yield return ancestor;
                    ancestor = ancestor.Parent;
                }
            }
        }

        public int ZIndex
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                if (Parent == null) return 0;
                return Parent.Children.IndexOf(this);
            }
        }

        public ViewContainer Root
        {
            get
            {
                ViewContainer container = this;
                while (container.Parent != null) container = container.Parent;
                return container;
            }
        }

        /// <summary>
        /// Gets the parent ViewContainer of this container.
        /// </summary>
        public ViewContainer Parent
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                return parent;
            }
            private set
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                parent = value;
            }
        }

        /// <summary>
        /// Gets the collection of this <see cref="View"/>'s children.
        /// </summary>
        public Collection<ViewContainer> Children
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                return children;
            }
        }

        /// <summary>
        /// Gets the sequence of <see cref="View"/> which are descendants of this one.
        /// </summary>
        public IEnumerable<ViewContainer> Descendants
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(null);
                foreach (ViewContainer child in children)
                {
                    yield return child;
                    foreach (ViewContainer childDescendant in child.Descendants)
                    {
                        yield return childDescendant;
                    }
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Tests if this <see cref="View"/> is within the children of another <see cref="View"/>,
        /// recursively.
        /// </summary>
        /// <param name="other">The other <see cref="View"/> to test against.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="View"/> is a descendant of <paramref name="other"/>,
        /// <c>false</c> if not.
        /// </returns>
        public bool IsDescendantOf(ViewContainer other)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            Argument.EnsureNotNull(other, "other");
            while (other != null)
            {
                if (other == this) return true;
                other = other.Parent;
            }
            return false;
        }

        /// <summary>
        /// Tests if this <see cref="View"/> is a the parent of another <see cref="View"/>,
        /// recursively.
        /// </summary>
        /// <param name="other">The other <see cref="View"/> to test against.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="View"/> is an ancestor of <paramref name="other"/>,
        /// <c>false</c> if not.
        /// </returns>
        public bool IsAncestorOf(ViewContainer other)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            Argument.EnsureNotNull(other, "other");
            return other.IsDescendantOf(this);
        }

        /// <summary>
        /// Removes this <see cref="View"/> from its parent. 
        /// </summary>
        public void RemoveFromParent()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            if (Parent != null) Parent.Children.Remove(this);
        }

        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            RemoveFromParent();
            while (children.Count > 0) children[0].Dispose();
            AncestryChanged = null;
            AddedToParent = null;
            RemovedFromParent = null;
            AddedChild = null;
            RemovedChild = null;
        }

        /// <summary>
        /// Renders this container.
        /// </summary>
        protected internal virtual void Render()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            foreach (ViewContainer container in Children)
            {
                container.Render();
            }
        }

        protected internal virtual void OnAddToParent(ViewContainer parent)
        {
            TriggerEvent(AddedToParent, parent);
            PropagateAncestryChangedEvent(parent);
        }

        protected internal virtual void OnRemoveFromParent(ViewContainer parent)
        {
            TriggerEvent(RemovedFromParent, parent);
            PropagateAncestryChangedEvent(parent);
        }

        protected internal virtual void OnAncestryChange(ViewContainer ancestor)
        {
            TriggerEvent(AncestryChanged, ancestor);
            PropagateAncestryChangedEvent(ancestor);
        }

        protected internal virtual void OnAddChild(ViewContainer child)
        {
            TriggerEvent(AddedChild, child);
        }

        protected internal virtual void OnRemoveChild(ViewContainer child)
        {
            TriggerEvent(RemovedChild, child);
        }

        private void TriggerEvent(GenericEventHandler<ViewContainer, ViewContainer> eventHandler, ViewContainer argument)
        {
            if (isDisposed) throw new ObjectDisposedException(null);
            if (eventHandler != null) eventHandler(this, argument);
        }

        protected internal virtual void PropagateAncestryChangedEvent(ViewContainer changingAncestor)
        {
            foreach (ViewContainer container in Children)
                container.OnAncestryChange(changingAncestor);
        }

        #endregion
    }
}
