using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Orion.Engine.Gui2
{
    partial class DockLayout
    {
        /// <summary>
        /// Holds the children of a <see cref="DockLayout"/>.
        /// </summary>
        public sealed class ChildCollection : Collection<DockedControl>
        {
            #region Fields
            private readonly DockLayout dock;
            #endregion

            #region Constructors
            internal ChildCollection(DockLayout dock)
            {
                Argument.EnsureNotNull(dock, "dock");

                this.dock = dock;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Gets a value indicating if a given <see cref="Control"/> is in this collection.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> to be found.</param>
            /// <returns><c>True</c> if the <see cref="Control"/> was found, <c>false</c> if not.</returns>
            public bool Contains(Control control)
            {
                return IndexOf(control) != -1;
            }

            /// <summary>
            /// Gets the index of a given <see cref="Control"/> in this collection.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> to be found.</param>
            /// <returns>The index of that <see cref="Control"/>, or <c>-1</c> if it was not found.</returns>
            public int IndexOf(Control control)
            {
                for (int i = 0; i < Count; ++i)
                    if (this[i].Control == control)
                        return i;

                return -1;
            }

            /// <summary>
            /// Removes a given <see cref="Control"/> from this collection.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> to be removed.</param>
            /// <returns><c>True</c> if the <see cref="Control"/> was removed, <c>false</c> if it was not found.</returns>
            public bool Remove(Control control)
            {
                int index = IndexOf(control);
                if (index == -1) return false;
                RemoveAt(index);
                return true;
            }

            /// <summary>
            /// Gets the <see cref="Dock"/> of a given <see cref="Control"/> in this collection.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> for which the <see cref="Dock"/> is to be retrieved.</param>
            /// <returns>The <see cref="Dock"/> of that <see cref="Control"/>.</returns>
            public Direction GetDock(Control control)
            {
                int index = IndexOf(control);
                if (index == -1) throw new KeyNotFoundException();

                return this[index].Dock;
            }

            /// <summary>
            /// Adds a new <see cref="Control"/> to this collection with the specified <see cref="Dock"/> value.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> to be added.</param>
            /// <param name="dock">The <see cref="Dock"/> value to be used.</param>
            public void Add(Control control, Direction dock)
            {
                Add(new DockedControl(control, dock));
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }

            protected override void InsertItem(int index, DockedControl item)
            {
                EnsureAddable(item);

                dock.AdoptChild(item.Control);
                base.InsertItem(index, item);
                dock.InvalidateMeasure();
            }

            protected override void RemoveItem(int index)
            {
                Control removedItem = this[index].Control;
                base.RemoveItem(index);

                dock.AbandonChild(removedItem);
                dock.InvalidateMeasure();
            }

            protected override void SetItem(int index, DockedControl item)
            {
                EnsureAddable(item);

                RemoveItem(index);
                InsertItem(index, item);
            }

            private void EnsureAddable(DockedControl item)
            {
                Argument.EnsureNotNull(item.Control, "item.Control");
                if (item.Control.Parent != null) throw new ArgumentException("Cannot add an item which already has a parent.");
            }
            #endregion
        }
    }
}
