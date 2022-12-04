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
        private readonly Collection<ViewContainer> children;
        private ViewContainer parent;
        private bool isDisposed;
        #endregion

        #region Constructors
        public ViewContainer()
        {
            this.children = new ViewChildrenCollection(this);
        }

        public ViewContainer(Collection<ViewContainer> children)
        {
            Argument.EnsureNotNull(children, "children");
            this.children = children;
        }
        #endregion

        #region Finalizer
        ~ViewContainer()
        {
            Dispose(false);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the parent has changed.
        /// The argument specifies the new parent.
        /// </summary>
        public event Action<ViewContainer, ViewContainer> AddedToParent;

        /// <summary>
        /// Raised when this view has been orphanized.
        /// The argument specifies the old parent.
        /// </summary>
        public event Action<ViewContainer, ViewContainer> RemovedFromParent;

        /// <summary>
        /// Raised when a child view has been added to this view.
        /// The argument specifies the view that was added.
        /// </summary>
        public event Action<ViewContainer, ViewContainer> ChildAdded;

        /// <summary>
        /// Raised when a child view has been removed from this view.
        /// The argument specifies the view that was removed.
        /// </summary>
        public event Action<ViewContainer, ViewContainer> ChildRemoved;

        /// <summary>
        /// Raised when one of the views in this views' ancestry has been added or removed.
        /// The argument specifies the view in the ancestry that changed.
        /// </summary>
        public event Action<ViewContainer, ViewContainer> AncestryChanged;

        /// <summary>
        /// Raised when this object has been disposed.
        /// </summary>
        public event Action<ViewContainer> Disposed;
        #endregion

        #region Properties
        public virtual Rectangle Frame { get; set; }
        public virtual Rectangle Bounds { get; set; }

        /// <summary>
        /// Gets the Z-Index of this view in its parent.
        /// 0 is the bottommost.
        /// </summary>
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
        /// <summary>
        /// Gets the parent view at the root of this container's ancestor tree.
        /// </summary>
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
        /// Gets the parent view of this container.
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
        /// Gets the collection of this view's children.
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
        /// Gets the sequence of views which are descendants of this one.
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

        /// <summary>
        /// Gets the sequence of views which are ancestors of this one.
        /// </summary>
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
                EnsureNotDisposed();
                RemoveFromParent();

                // Children are removed this way as they detach from their parent (this) when removed.
                while (children.Count > 0) children[0].Dispose();

                AddedToParent = null;
                RemovedFromParent = null;
                ChildAdded = null;
                ChildRemoved = null;
                AncestryChanged = null;

                GC.SuppressFinalize(this);

                isDisposed = true;

                Disposed.Raise(this);
                Disposed = null;
            }
            else
            {
                // Use an assert if being finalized because the finalizer thread
                // is not a nice place to throw an exception.
                Debug.Assert(!isDisposed);
                isDisposed = true;
            }
        }

        protected void EnsureNotDisposed()
        {
#warning Cannot properly implement EnsureNotDisposed as many objects depend on usage of disposed views :/
            Debug.Assert(!isDisposed, "A view is being used after being disposed.");
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
            AddedToParent.Raise(this, parent);
            PropagateAncestryChangedEvent(parent);
        }

        protected internal virtual void OnRemovedFromParent(ViewContainer oldParent)
        {
            RemovedFromParent.Raise(this, oldParent);
            PropagateAncestryChangedEvent(oldParent);
        }

        protected internal virtual void OnAncestryChanged(ViewContainer ancestor)
        {
            AncestryChanged.Raise(this, ancestor);
            PropagateAncestryChangedEvent(ancestor);
        }

        protected internal virtual void OnChildAdded(ViewContainer child)
        {
            ChildAdded.Raise(this, child);
        }

        protected internal virtual void OnChildRemoved(ViewContainer child)
        {
            ChildRemoved.Raise(this, child);
        }

        protected internal virtual void PropagateAncestryChangedEvent(ViewContainer changingAncestor)
        {
            foreach (ViewContainer container in Children)
                container.OnAncestryChanged(changingAncestor);
        }
        #endregion
    }
}
