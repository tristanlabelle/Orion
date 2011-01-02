using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A panel which arranges its child <see cref="Control"/>s within a grid with uniform-sized cells.
    /// </summary>
    public sealed partial class UniformGridPanel : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private readonly int[] rowHeights;
        private readonly int[] columnWidths;
        private int cellGap;
        #endregion

        #region Constructors
        public UniformGridPanel(int rowCount, int columnCount)
        {
            this.children = new ChildCollection(this, rowCount, columnCount);
            this.rowHeights = new int[rowCount];
            this.columnWidths = new int[columnCount];
        }
        #endregion

        #region Properties
        public new ChildCollection Children
        {
            get { return children; }
        }

        public int RowCount
        {
            get { return children.RowCount; }
        }

        public int ColumnCount
        {
            get { return children.ColumnCount; }
        }

        public int CellGap
        {
            get { return cellGap; }
            set
            {
                if (value == cellGap) return;
                Argument.EnsurePositive(value, "CellGap");
                cellGap = value;
                InvalidateMeasure();
            }
        }
        #endregion

        #region Methods
        protected override IEnumerable<Control> GetChildren()
        {
            return children;
        }

        protected override Size MeasureSize()
        {
            Array.Clear(rowHeights, 0, rowHeights.Length);
            Array.Clear(columnWidths, 0, columnWidths.Length);

            for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                {
                    if (children[rowIndex, columnIndex] != null)
                    {
                        Size size = children[rowIndex, columnIndex].Measure();
                        if (size.Height > rowHeights[rowIndex]) rowHeights[rowIndex] = size.Height;
                        if (size.Width > columnWidths[columnIndex]) columnWidths[columnIndex] = size.Width;
                    }
                }
            }

            return new Size(
                columnWidths.Sum() + Math.Max(0, ColumnCount - 1) * cellGap,
                rowHeights.Sum() + Math.Max(0, RowCount - 1) * cellGap);
        }

        protected override void ArrangeChildren()
        {
            Measure();

            Region rectangle = Rectangle;

            int y = rectangle.MinY;
            for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
            {
                if (rowIndex > 0) y += cellGap;

                int x = rectangle.MinX;
                for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                {
                    if (columnIndex > 0) x += cellGap;

                    Control child = children[rowIndex, columnIndex];
                    if (child != null)
                    {
                        Size size = child.DesiredOuterSize;

                        Region cellRectangle = new Region(x, y, columnWidths[columnIndex], rowHeights[rowIndex]);
                        DefaultArrangeChild(child, cellRectangle);
                    }
                    
                    x += columnWidths[columnIndex];
                }

                y += rowHeights[rowIndex];
            }
        }
        #endregion
    }
}
