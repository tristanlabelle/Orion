using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    partial class UniformGridPanel
    {
        public sealed class ChildCollection : ICollection<Control>
        {
            #region Fields
            private readonly UniformGridPanel panel;
            private readonly Control[,] children;
            private int count;
            #endregion

            #region Constructors
            internal ChildCollection(UniformGridPanel panel, int rowCount, int columnCount)
            {
                Argument.EnsureNotNull(panel, "panel");
                Argument.EnsurePositive(rowCount, "rowCount");
                Argument.EnsurePositive(columnCount, "columnCount");

                this.panel = panel;
                this.children = new Control[rowCount, columnCount];
            }
            #endregion

            #region Properties
            public int RowCount
            {
                get { return children.GetLength(0); }
            }

            public int ColumnCount
            {
                get { return children.GetLength(1); }
            }

            public int CellCount
            {
                get { return children.Length; }
            }

            public int Count
            {
                get { return count; }
            }
            #endregion

            #region Indexers
            public Control this[int rowIndex, int columnIndex]
            {
                get { return children[rowIndex, columnIndex]; }
                set
                {
                    RemoveAt(rowIndex, columnIndex);
                    panel.AdoptChild(value);
                    children[rowIndex, columnIndex] = value;
                }
            }
            #endregion

            #region Methods
            public void Add(Control item)
            {
                Argument.EnsureNotNull(item, "item");
                if (Count == CellCount) throw new InvalidOperationException("Cannot add a child to a full UniformGridPanel");

                panel.AdoptChild(item);
                for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
                {
                    for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                    {
                        if (children[rowIndex, columnIndex] == null)
                        {
                            children[rowIndex, columnIndex] = item;
                            ++count;
                            return;
                        }
                    }
                }

                Debug.Fail("The child control wasn't added as it was supposed.");
            }

            public void Clear()
            {
                if (count == 0) return;

                for (int rowIndex = RowCount - 1; rowIndex >= 0; --rowIndex)
                {
                    for (int columnIndex = ColumnCount - 1; columnIndex >= 0; --columnIndex)
                    {
                        if (children[rowIndex, columnIndex] != null)
                        {
                            RemoveAt(rowIndex, columnIndex);
                        }
                    }
                }
            }

            public bool Find(Control item, out int rowIndex, out int columnIndex)
            {
                if (item != null)
                    for (rowIndex = 0; rowIndex < RowCount; ++rowIndex)
                        for (columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                            if (children[rowIndex, columnIndex] == item)
                                return true;

                rowIndex = -1;
                columnIndex = -1;
                return false;
            }

            public bool Contains(Control item)
            {
                int rowIndex, columnIndex;
                return Find(item, out rowIndex, out columnIndex);
            }

            public bool Remove(Control item)
            {
                int rowIndex, columnIndex;
                if (!Find(item, out rowIndex, out columnIndex))
                    return false;

                RemoveAt(rowIndex, columnIndex);
                return true;
            }

            public bool RemoveAt(int rowIndex, int columnIndex)
            {
                Control child = children[rowIndex, columnIndex];
                if (child == null) return false;

                children[rowIndex, columnIndex] = null;
                panel.AbandonChild(child);
                return true;
            }

            public IEnumerator<Control> GetEnumerator()
            {
                for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
                    for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                        if (children[rowIndex, columnIndex] != null)
                            yield return children[rowIndex, columnIndex];
            }
            #endregion

            #region Explicit Members
            void ICollection<Control>.CopyTo(Control[] array, int arrayIndex)
            {
                int offset = 0;
                for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
                {
                    for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                    {
                        if (children[rowIndex, columnIndex] != null)
                        {
                            array[arrayIndex + offset] = children[rowIndex, columnIndex];
                            ++offset;
                        }
                    }
                }
            }

            bool ICollection<Control>.IsReadOnly
            {
                get { return false; }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
    }
}
