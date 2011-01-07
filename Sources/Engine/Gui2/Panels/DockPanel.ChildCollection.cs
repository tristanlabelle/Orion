using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Orion.Engine.Gui2
{
    partial class DockPanel
    {
        /// <summary>
        /// Holds the children of a <see cref="DockPanel"/>.
        /// </summary>
        public sealed class ChildCollection : Collection<DockedControl>
        {
            public sealed class ControlCollection : ICollection<Control>
            {
                #region Fields
                private readonly ChildCollection collection;
                #endregion

                #region Constructors
                internal ControlCollection(ChildCollection collection)
                {
                    Argument.EnsureNotNull(collection, "collection");
                    this.collection = collection;
                }
                #endregion

                #region Properties
                public int Count
                {
                    get { return collection.Count; }
                }
                #endregion

                #region Methods
                public void Clear()
                {
                    collection.Clear();
                }

                public bool Contains(Control item)
                {
                    return collection.Contains(item);
                }

                public void CopyTo(Control[] array, int arrayIndex)
                {
                    for (int i = 0; i < collection.Count; ++i)
                        array[arrayIndex + i] = collection[i].Control;
                }

                public bool Remove(Control item)
                {
                    return collection.Remove(item);
                }

                public IEnumerator<Control> GetEnumerator()
                {
                    return collection.Select(item => item.Control).GetEnumerator();
                }
                #endregion

                #region Explicit Members
                void ICollection<Control>.Add(Control item)
                {
                    throw new NotSupportedException();
                }

                bool ICollection<Control>.IsReadOnly
                {
                    get { return false; }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
                #endregion
            }

            #region Fields
            private readonly DockPanel panel;
            private readonly ControlCollection controls;
            #endregion

            #region Constructors
            internal ChildCollection(DockPanel panel)
            {
                Argument.EnsureNotNull(panel, "panel");

                this.panel = panel;
                this.controls = new ControlCollection(this);
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the collection of <see cref="Control"/>s within this collection.
            /// </summary>
            public ControlCollection Controls
            {
                get { return controls; }
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

                panel.AdoptChild(item.Control);
                base.InsertItem(index, item);
                panel.InvalidateMeasure();
            }

            protected override void RemoveItem(int index)
            {
                Control removedItem = this[index].Control;
                base.RemoveItem(index);

                panel.AbandonChild(removedItem);
                panel.InvalidateMeasure();
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
