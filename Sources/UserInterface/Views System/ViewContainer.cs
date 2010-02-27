using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Orion.Geometry;
using Orion.Engine.Graphics;

namespace Orion.UserInterface
{
    public abstract class ViewContainer : IDisposable
    {
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

        #region Finalizer
        ~ViewContainer()
        {
            Dispose(false);
        }
        #endregion

        #region Events
        public event Action<ViewContainer, ViewContainer> AddedToParent;
        public event Action<ViewContainer, ViewContainer> RemovedFromParent;
        public event Action<ViewContainer, ViewContainer> ChildAdded;
        public event Action<ViewContainer, ViewContainer> ChildRemoved;
        public event Action<ViewContainer, ViewContainer> AncestryChanged;
        public event Action<ViewContainer> Disposed;

        private void RaiseEvent(Action<ViewContainer, ViewContainer> eventHandler, ViewContainer arg)
        {
            EnsureNotDisposed();
            if (eventHandler != null) eventHandler(this, arg);
        }
        #endregion

        #region Properties
        public virtual Rectangle Frame { get; set; }
        public virtual Rectangle Bounds { get; set; }

        public int ZIndex
        {
            get
            {
                EnsureNotDisposed();
                if (Parent == null) return 0;
                return Parent.Children.IndexOf(this);
            }
        }

        #region Hierarchy
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
                EnsureNotDisposed();
                return parent;
            }
            internal set
            {
                EnsureNotDisposed();
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
                EnsureNotDisposed();
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
                EnsureNotDisposed();
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

        public IEnumerable<ViewContainer> Ancestors
        {
            get
            {
                EnsureNotDisposed();
                ViewContainer ancestor = Parent;
                while (ancestor != null)
                {
                    yield return ancestor;
                    ancestor = ancestor.Parent;
                }
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Hierarchy
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
            EnsureNotDisposed();
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
            EnsureNotDisposed();
            Argument.EnsureNotNull(other, "other");
            return other.IsDescendantOf(this);
        }

        /// <summary>
        /// Removes this <see cref="View"/> from its parent. 
        /// </summary>
        public void RemoveFromParent()
        {
            EnsureNotDisposed();
            if (Parent != null) Parent.Children.Remove(this);
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Disposes this object, releasing all used resources.
        /// </summary>
        public void Dispose()
        {
            Debug.WriteLine("{0} is being disposed", this.ToString());
            EnsureNotDisposed();

            try
            {
                Dispose(true);
            }
            finally
            {
                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this <see cref="ViewContainer"/>.
        /// </summary>
        /// <param name="disposing">
        /// <c>True</c> if the object is explicitly disposed,
        /// <c>false</c> if this object is being finalized.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveFromParent();
                while (children.Count > 0) children[0].Dispose();
                AddedToParent = null;
                RemovedFromParent = null;
                ChildAdded = null;
                ChildRemoved = null;
                AncestryChanged = null;

                var disposedEventHandler = Disposed;
                if (disposedEventHandler != null)
                {
                    Disposed = null;
                    disposedEventHandler(this);
                }

                GC.SuppressFinalize(this);
            }
        }

        protected void EnsureNotDisposed()
        {
#warning Cannot properly implement EnsureNotDisposed as many objects depend on usage of disposed objects :/
            //if (isDisposed) throw new ObjectDisposedException(null);
        }
        #endregion

        /// <summary>
        /// Renders this container.
        /// </summary>
        protected internal virtual void Render(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            EnsureNotDisposed();
            foreach (ViewContainer container in Children)
                container.Render(graphicsContext);
        }

        protected internal virtual void OnAddToParent(ViewContainer parent)
        {
            RaiseEvent(AddedToParent, parent);
            PropagateAncestryChangedEvent(parent);
        }

        protected internal virtual void OnRemovedFromParent(ViewContainer oldParent)
        {
            RaiseEvent(RemovedFromParent, oldParent);
            PropagateAncestryChangedEvent(oldParent);
        }

        protected internal virtual void OnAncestryChanged(ViewContainer ancestor)
        {
            RaiseEvent(AncestryChanged, ancestor);
            PropagateAncestryChangedEvent(ancestor);
        }

        protected internal virtual void OnChildAdded(ViewContainer child)
        {
            RaiseEvent(ChildAdded, child);
        }

        protected internal virtual void OnChildRemoved(ViewContainer child)
        {
            RaiseEvent(ChildRemoved, child);
        }

        protected internal virtual void PropagateAncestryChangedEvent(ViewContainer changingAncestor)
        {
            foreach (ViewContainer container in Children)
                container.OnAncestryChanged(changingAncestor);
        }
        #endregion
    }
}
