using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A panel which arranges its child <see cref="Control"/>s within a grid cells.
    /// </summary>
    public sealed partial class GridPanel : Control
    {
        #region Fields
        private readonly ChildCollection children;
        private bool areRowsUniformSized;
        private bool areColumnsUniformSized;
        private int cellGap;
        private int[] desiredRowHeights;
        private int[] desiredColumnWidths;
        #endregion

        #region Constructors
        public GridPanel(int rowCount, int columnCount)
        {
            this.children = new ChildCollection(this, rowCount, columnCount);
            this.desiredRowHeights = new int[rowCount];
            this.desiredColumnWidths = new int[columnCount];
        }

        public GridPanel() : this(0, 0) { }
        #endregion

        #region Properties
        public new ChildCollection Children
        {
            get { return children; }
        }

        public bool AreRowsUniformSized
        {
            get { return areRowsUniformSized; }
            set
            {
                if (value == areRowsUniformSized) return;
                areRowsUniformSized = value;
                InvalidateMeasure();
            }
        }

        public bool AreColumnsUniformSized
        {
            get { return areColumnsUniformSized; }
            set
            {
                if (value == areColumnsUniformSized) return;
                areColumnsUniformSized = value;
                InvalidateMeasure();
            }
        }

        public int RowCount
        {
            get { return children.RowCount; }
            set { children.RowCount = value; }
        }

        public int ColumnCount
        {
            get { return children.ColumnCount; }
            set { children.ColumnCount = value; }
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
            Array.Clear(desiredRowHeights, 0, desiredRowHeights.Length);
            Array.Clear(desiredColumnWidths, 0, desiredColumnWidths.Length);

            for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                {
                    if (children[rowIndex, columnIndex] != null)
                    {
                        Size size = children[rowIndex, columnIndex].Measure();
                        if (size.Height > desiredRowHeights[rowIndex]) desiredRowHeights[rowIndex] = size.Height;
                        if (size.Width > desiredColumnWidths[columnIndex]) desiredColumnWidths[columnIndex] = size.Width;
                    }
                }
            }

            int totalRowHeights;
            if (RowCount == 0) totalRowHeights = 0;
            else if (AreRowsUniformSized) totalRowHeights = desiredColumnWidths.Max() * RowCount;
            else totalRowHeights = desiredColumnWidths.Sum();

            int totalColumnWidth;
            if (ColumnCount == 0) totalColumnWidth = 0;
            else if (AreColumnsUniformSized) totalColumnWidth = desiredColumnWidths.Max() * ColumnCount;
            else totalColumnWidth = desiredColumnWidths.Sum();

            return new Size(
                totalColumnWidth + Math.Max(0, ColumnCount - 1) * cellGap,
                totalRowHeights + Math.Max(0, RowCount - 1) * cellGap);
        }

        protected override void ArrangeChildren()
        {
            Measure();

            Region rectangle = Rectangle;

            int rowHeight = Math.Max(0, (rectangle.Height - Math.Max(0, RowCount - 1) * cellGap) / RowCount);
            int columnWidth = Math.Max(0, (rectangle.Width - Math.Max(0, ColumnCount - 1) * cellGap) / ColumnCount);

            int y = rectangle.MinY;
            for (int rowIndex = 0; rowIndex < RowCount; ++rowIndex)
            {
                if (rowIndex > 0) y += cellGap;

                if (!AreRowsUniformSized) rowHeight = desiredRowHeights[rowIndex];

                int x = rectangle.MinX;
                for (int columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                {
                    if (columnIndex > 0) x += cellGap;

                    if (!AreColumnsUniformSized) columnWidth = desiredColumnWidths[columnIndex];

                    Control child = children[rowIndex, columnIndex];
                    if (child != null)
                    {
                        Size size = child.DesiredOuterSize;

                        Region cellRectangle = new Region(x, y, columnWidth, rowHeight);
                        DefaultArrangeChild(child, cellRectangle);
                    }

                    x += columnWidth;
                }

                y += rowHeight;
            }
        }
        #endregion
    }
}
