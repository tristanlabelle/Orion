using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Gui
{
    // This class part defines members relating to the UI hierarchy (ancestors and descendants).
    partial class Control
    {
        #region Fields
        private UIManager manager;
        private Control parent;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="UIManager"/> at the root of this UI hierarchy.
        /// </summary>
        public UIManager Manager
        {
            get { return manager; }
        }

        /// <summary>
        /// Gets the <see cref="Control"/> which contains this <see cref="Control"/> in the UI hierarchy.
        /// </summary>
        public Control Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Enumerates the ancestors of this <see cref="Control"/>.
        /// </summary>
        public IEnumerable<Control> Ancestors
        {
            get
            {
                Control ancestor = parent;
                while (ancestor != null)
                {
                    yield return ancestor;
                    ancestor = ancestor.parent;
                }
            }
        }

        /// <summary>
        /// Enumerates the children of this <see cref="Control"/>.
        /// </summary>
        /// <remarks>
        /// This is implemented through <see cref="GetChildren"/> to allow overriding and shadowing simultaneously in a derived class.
        /// </remarks>
        public IEnumerable<Control> Children
        {
            get { return GetChildren(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Obtains the sequence of children of this <see cref="Control"/>.
        /// </summary>
        /// <returns>A sequence of the children of this <see cref="Control"/>.</returns>
        protected virtual IEnumerable<Control> GetChildren()
        {
            return Enumerable.Empty<Control>();
        }

        /// <summary>
        /// Finds a direct child of this <see cref="Control"/> from a point.
        /// </summary>
        /// <param name="point">A point where the child should be, in absolute coordinates.</param>
        /// <returns>The child at that point, or <c>null</c> if no child can be found at that point.</returns>
        public virtual Control GetChildAt(Point point)
        {
            if (!Rectangle.Contains(point) || Visibility < Visibility.Visible) return null;

            foreach (Control child in Children)
                if (child.VisibilityFlag == Gui.Visibility.Visible && child.Rectangle.Contains(point))
                    return child;

            return null;
        }

        /// <summary>
        /// Gets the deepest descendant <see cref="Control"/> at a given location.
        /// </summary>
        /// <param name="point">The location where to find the descendant.</param>
        /// <returns>The deepest descendant at that location.</returns>
        public Control GetDescendantAt(Point point)
        {
            Control current = this;
            while (true)
            {
                Control descendant = current.GetChildAt(point);
                if (descendant == null) break;
                current = descendant;
            }

            return current;
        }

        /// <summary>
        /// Determines a given <see cref="Control"/> is an ancestor of this <see cref="Control"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be tested.</param>
        /// <returns><c>True</c> if it is this <see cref="Control"/> or one of its ancestors, <c>false</c> if not.</returns>
        public bool HasAncestor(Control control)
        {
            if (control == null) return false;

            Control ancestor = this;
            while (true)
            {
                if (ancestor == control) return true;
                ancestor = ancestor.parent;
                if (ancestor == null) return false;
            }
        }

        /// <summary>
        /// Determines a given <see cref="Control"/> is a descendant of this <see cref="Control"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be tested.</param>
        /// <returns><c>True</c> if it is this <see cref="Control"/> or one of its descendants, <c>false</c> if not.</returns>
        public bool HasDescendant(Control control)
        {
            while (true)
            {
                if (control == null) return false;
                if (control == this) return true;
                control = control.Parent;
            }
        }

        /// <summary>
        /// Changes the parent of this <see cref="Control"/> in the UI hierarchy.
        /// </summary>
        /// <param name="parent">The new parent of this <see cref="Control"/>.</param>
        private void SetParent(Control parent)
        {
            if (this is UIManager) throw new InvalidOperationException("The UI manager cannot be a child.");
            if (this.parent != null && parent != null)
                throw new InvalidOperationException("Cannot set the parent when already parented.");

            isMeasured = false;
            isArranged = false;
            this.parent = parent;
            UIManager newManager = parent == null ? null : parent.manager;
            if (newManager != manager) SetManagerRecursively(newManager);
        }

        private void SetManagerRecursively(UIManager manager)
        {
            UIManager previousManager = this.manager;
            this.manager = manager;
            OnManagerChanged(previousManager);

            foreach (Control child in Children)
                child.SetManagerRecursively(manager);
        }

        protected virtual void OnManagerChanged(UIManager previousManager) { }

        protected void AdoptChild(Control child)
        {
            Argument.EnsureNotNull(child, "child");
            if (child.parent == this) return;
            if (child.parent != null) throw new ArgumentException("Cannot add a child which already has another parent.");

            child.SetParent(this);
        }

        protected void AbandonChild(Control child)
        {
            Debug.Assert(child.Parent == this);
            child.SetParent(null);
        }

        /// <summary>
        /// Finds the common ancestor of two <see cref="Control"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// The common ancestor of those <see cref="Control"/>s,
        /// or <c>null</c> if they have no common ancestor or one of them is <c>null</c>.
        /// </returns>
        public static Control FindCommonAncestor(Control a, Control b)
        {
            Control ancestorA = a;
            while (ancestorA != null)
            {
                Control ancestorB = b;
                while (ancestorB != null)
                {
                    if (ancestorB == ancestorA) return ancestorA;
                    ancestorB = ancestorB.Parent;
                }
                ancestorA = ancestorA.Parent;
            }
            return null;
        }
        #endregion
    }
}
