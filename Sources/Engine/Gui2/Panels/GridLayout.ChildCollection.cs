using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Gui2
{
    partial class GridLayout
    {
        public sealed class ChildCollection : ICollection<Control>
        {
            #region Fields
            private readonly GridLayout grid;
            private Control[,] children;
            private int rowCount;
            private int columnCount;
            private int count;
            #endregion

            #region Constructors
            internal ChildCollection(GridLayout grid, int rowCount, int columnCount)
            {
                Argument.EnsureNotNull(grid, "grid");
                Argument.EnsurePositive(rowCount, "rowCount");
                Argument.EnsurePositive(columnCount, "columnCount");

                this.grid = grid;
                this.children = new Control[rowCount, columnCount];
                this.rowCount = rowCount;
                this.columnCount = columnCount;
            }
            #endregion

            #region Properties
            public int RowCount
            {
                get { return rowCount; }
                set { Resize(value, columnCount); }
            }

            public int ColumnCount
            {
                get { return columnCount; }
                set { Resize(rowCount, value); }
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
                    grid.AdoptChild(value);
                    children[rowIndex, columnIndex] = value;
                }
            }
            #endregion

            #region Methods
            public void Resize(int newRowCount, int newColumnCount)
            {
                Argument.EnsurePositive(newRowCount, "newRowCount");
                Argument.EnsurePositive(newColumnCount, "newColumnCount");

                if (newRowCount < RowCount || newColumnCount < ColumnCount)
                {
                    for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
                        for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                            if (rowIndex >= newRowCount || columnIndex >= newColumnCount)
                                RemoveAt(rowIndex, columnIndex);
                }

                if (newRowCount > children.GetLength(0) || newColumnCount > children.GetLength(1))
                {
                    Control[,] newChildren = new Control[
                        Math.Max(children.GetLength(0), newRowCount),
                        Math.Max(children.GetLength(1), newColumnCount)];

                    for (int rowIndex = 0; rowIndex < newRowCount; ++rowIndex)
                        for (int columnIndex = 0; columnIndex < newColumnCount; ++columnIndex)
                            newChildren[rowIndex, columnIndex] = children[rowIndex, columnIndex];

                    children = newChildren;
                }

                this.rowCount = newRowCount;
                this.columnCount = newColumnCount;
            }

            public void Add(Control item)
            {
                Argument.EnsureNotNull(item, "item");
                if (Count == CellCount) throw new InvalidOperationException("Cannot add a child to a full grid.");

                grid.AdoptChild(item);
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
                grid.AbandonChild(child);
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
