using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Orion.UserInterface
{
    public abstract class ViewContainer
    {
        #region Fields
        private Collection<ViewContainer> children;
        private ViewContainer parent;
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

        #region Properties
        public IEnumerable<ViewContainer> Ancestors
        {
            get
            {
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
                if (Parent == null) return 0;
                return Parent.Children.IndexOf(this);
            }
        }

        /// <summary>
        /// Gets the parent ViewContainer of this container.
        /// </summary>
        public ViewContainer Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets the collection of this <see cref="View"/>'s children.
        /// </summary>
        public Collection<ViewContainer> Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets the sequence of <see cref="View"/> which are descendants of this one.
        /// </summary>
        public IEnumerable<ViewContainer> Descendants
        {
            get
            {
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
            Argument.EnsureNotNull(other, "other");
            return other.IsDescendantOf(this);
        }

        /// <summary>
        /// Removes this <see cref="View"/> from its parent. 
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent != null) Parent.Children.Remove(this);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Renders this container.
        /// </summary>
        protected internal virtual void Render()
        {
            foreach (ViewContainer container in Children)
            {
                container.Render();
            }
        }

        #endregion
    }
}
