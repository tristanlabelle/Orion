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
        public sealed class ChildCollection : Collection<DockedElement>
        {
            public sealed class ElementCollection : ICollection<UIElement>
            {
                #region Fields
                private readonly ChildCollection collection;
                #endregion

                #region Constructors
                internal ElementCollection(ChildCollection collection)
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

                public bool Contains(UIElement item)
                {
                    return collection.Contains(item);
                }

                public void CopyTo(UIElement[] array, int arrayIndex)
                {
                    for (int i = 0; i < collection.Count; ++i)
                        array[arrayIndex + i] = collection[i].Element;
                }

                public bool Remove(UIElement item)
                {
                    return collection.Remove(item);
                }

                public IEnumerator<UIElement> GetEnumerator()
                {
                    return collection.Select(item => item.Element).GetEnumerator();
                }
                #endregion

                #region Explicit Members
                void ICollection<UIElement>.Add(UIElement item)
                {
                    throw new NotSupportedException();
                }

                bool ICollection<UIElement>.IsReadOnly
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
            private readonly ElementCollection elements;
            #endregion

            #region Constructors
            internal ChildCollection(DockPanel panel)
            {
                Argument.EnsureNotNull(panel, "panel");

                this.panel = panel;
                this.elements = new ElementCollection(this);
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the collection of elements within this collection.
            /// </summary>
            public ElementCollection Elements
            {
                get { return elements; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Gets a value indicating if a given <see cref="UIElement"/> is in this collection.
            /// </summary>
            /// <param name="element">The <see cref="UIElement"/> to be found.</param>
            /// <returns><c>True</c> if the <see cref="UIElement"/> was found, <c>false</c> if not.</returns>
            public bool Contains(UIElement element)
            {
                return IndexOf(element) != -1;
            }

            /// <summary>
            /// Gets the index of a given <see cref="UIElement"/> in this collection.
            /// </summary>
            /// <param name="element">The <see cref="UIElement"/> to be found.</param>
            /// <returns>The index of that <see cref="UIElement"/>, or <c>-1</c> if it was not found.</returns>
            public int IndexOf(UIElement element)
            {
                for (int i = 0; i < Count; ++i)
                    if (this[i].Element == element)
                        return i;

                return -1;
            }

            /// <summary>
            /// Removes a given <see cref="UIElement"/> from this collection.
            /// </summary>
            /// <param name="element">The <see cref="UIElement"/> to be removed.</param>
            /// <returns><c>True</c> if the <see cref="UIElement"/> was removed, <c>false</c> if it was not found.</returns>
            public bool Remove(UIElement element)
            {
                int index = IndexOf(element);
                if (index == -1) return false;
                RemoveAt(index);
                return true;
            }

            /// <summary>
            /// Gets the <see cref="Dock"/> of a given <see cref="UIElement"/> in this collection.
            /// </summary>
            /// <param name="element">The <see cref="UIElement"/> for which the <see cref="Dock"/> is to be retrieved.</param>
            /// <returns>The <see cref="Dock"/> of that <see cref="UIElement"/>.</returns>
            public Dock GetDock(UIElement element)
            {
                int index = IndexOf(element);
                if (index == -1) throw new KeyNotFoundException();

                return this[index].Dock;
            }

            /// <summary>
            /// Adds a new <see cref="UIElement"/> to this collection with the specified <see cref="Dock"/> value.
            /// </summary>
            /// <param name="element">The <see cref="UIElement"/> to be added.</param>
            /// <param name="dock">The <see cref="Dock"/> value to be used.</param>
            public void Add(UIElement element, Dock dock)
            {
                Add(new DockedElement(element, dock));
            }

            protected override void ClearItems()
            {
                while (Count > 0) RemoveItem(Count - 1);
            }

            protected override void InsertItem(int index, DockedElement item)
            {
                EnsureAddable(item);

                panel.AdoptChild(item.Element);
                base.InsertItem(index, item);
                panel.InvalidateMeasure();
            }

            protected override void RemoveItem(int index)
            {
                UIElement removedItem = this[index].Element;
                base.RemoveItem(index);

                panel.AbandonChild(removedItem);
                panel.InvalidateMeasure();
            }

            protected override void SetItem(int index, DockedElement item)
            {
                EnsureAddable(item);

                RemoveItem(index);
                InsertItem(index, item);
            }

            private void EnsureAddable(DockedElement item)
            {
                Argument.EnsureNotNull(item.Element, "item.Element");
                if (item.Element.Parent != null) throw new ArgumentException("Cannot add an item which already has a parent.");
            }
            #endregion
        }
    }
}
