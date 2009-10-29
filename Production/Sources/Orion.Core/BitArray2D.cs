using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion
{
    /// <summary>
    /// Compactly stores a 2D array of bits.
    /// </summary>
    [Serializable]
    public sealed class BitArray2D : IEnumerable<bool>
    {
        #region Instance
        #region Fields
        private readonly BitArray bits;
        private readonly int columnCount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance with the specified dimensions and a default value for the bits.
        /// </summary>
        /// <param name="rowCount">The number of rows this bit array contains.</param>
        /// <param name="columnCount">The number of columns this bit array contains.</param>
        /// <param name="defaultValue">The default value of the bits in this array.</param>
        public BitArray2D(int rowCount, int columnCount, bool defaultValue)
        {
            Argument.EnsurePositive(rowCount, "rowCount");
            Argument.EnsurePositive(columnCount, "columnCount");

            this.bits = new BitArray(rowCount * columnCount, defaultValue);
            this.columnCount = columnCount;
        }

        /// <summary>
        /// Initializes a new instance with the specified dimensions.
        /// </summary>
        /// <param name="rowCount">The number of rows this bit array contains.</param>
        /// <param name="columnCount">The number of columns this bit array contains.</param>
        public BitArray2D(int rowCount, int columnCount)
            : this(rowCount, columnCount, false) { }

        /// <summary>
        /// Initializes a new instance from an array of bits to be copied.
        /// </summary>
        /// <param name="bits">The bits to be copied.</param>
        /// <param name="rowCount">The number of bit rows.</param>
        /// <param name="columnCount">The number of bit columns.</param>
        public BitArray2D(BitArray bits, int rowCount, int columnCount)
        {
            Argument.EnsureNotNull(bits, "bits");
            Argument.EnsurePositive(rowCount, "rowCount");
            Argument.EnsurePositive(columnCount, "columnCount");

            if (bits.Length != rowCount * columnCount)
                throw new ArgumentException("Row and column count do not match bit array size.");

            this.bits = (BitArray)bits.Clone();
            this.columnCount = columnCount;
        }

        /// <summary>
        /// Initializes a new instance from an array of bits to be copied.
        /// </summary>
        /// <param name="bits">The bits to be copied.</param>
        /// <param name="rowCount">The number of bit rows.</param>
        /// <param name="columnCount">The number of bit columns.</param>
        public BitArray2D(bool[] bits, int rowCount, int columnCount)
        {
            Argument.EnsureNotNull(bits, "bits");
            Argument.EnsurePositive(rowCount, "rowCount");
            Argument.EnsurePositive(columnCount, "columnCount");


            if (bits.Length != rowCount * columnCount)
                throw new ArgumentException("Row and column count do not match bit array size.");

            this.bits = new BitArray(bits);
            this.columnCount = columnCount;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the underlying bit array.
        /// </summary>
        public BitArray Bits
        {
            get { return bits; }
        }

        /// <summary>
        /// Gets the number of rows this bit array contains.
        /// </summary>
        public int RowCount
        {
            get { return bits.Length / columnCount; }
        }

        /// <summary>
        /// Gets the number of columns this bit array contains.
        /// </summary>
        public int ColumnCount
        {
            get { return columnCount; }
        }

        /// <summary>
        /// Gets the length of this bit array, in bits.
        /// </summary>
        public int Length
        {
            get { return bits.Length; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses a bit at a given location in this array.
        /// </summary>
        /// <param name="rowIndex">The index of the bit's row.</param>
        /// <param name="columnIndex">The index of the bit's column.</param>
        /// <returns>The value of the bit at that location.</returns>
        public bool this[int rowIndex, int columnIndex]
        {
            get { return bits[GetIndex(rowIndex, columnIndex)]; }
            set { bits[GetIndex(rowIndex, columnIndex)] = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clones this 2D bit array.
        /// </summary>
        /// <returns>The bit array to be cloned.</returns>
        public BitArray2D Clone()
        {
            return new BitArray2D(bits, RowCount, columnCount);
        }

        /// <summary>
        /// Gets an enumerator which iterates over the values of this sequence.
        /// </summary>
        /// <returns>A new enumerator.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Inverts the value of the bits in this array.
        /// </summary>
        public void Invert()
        {
            bits.Not();
        }

        /// <summary>
        /// Sets all the bits in this array to a given value.
        /// </summary>
        /// <param name="value">The value to which bits are to be set.</param>
        public void SetAll(bool value)
        {
            bits.SetAll(value);
        }

        public override string ToString()
        {
            return "{0}x{1} bits".FormatInvariant(RowCount, ColumnCount);
        }

        private int GetIndex(int rowIndex, int columnIndex)
        {
            return rowIndex * columnCount + columnIndex;
        }
        #endregion

        #region Explicit Members
        #region IEnumerable<bool> Members
        IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A bit array with dimensions of 0 by 0.
        /// </summary>
        public static readonly BitArray2D Empty = new BitArray2D(0, 0);
        #endregion
        #endregion

        #region Nested Types
        /// <summary>
        /// An enumerator for <see cref="BitArray2D"/>.
        /// </summary>
        [Serializable]
        public struct Enumerator : IEnumerator<bool>
        {
            #region Fields
            private BitArray2D array;
            private int index;
            #endregion

            #region Constructors
            internal Enumerator(BitArray2D array)
            {
                this.array = array;
                this.index = -1;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the value this enumerator currently points to.
            /// </summary>
            public bool Current
            {
                get { return array.bits[index]; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Moves to the next value within the enumerated sequence.
            /// </summary>
            /// <returns>
            /// True if the operation was successful, false if the enumerator has reached the end of the sequence.
            /// </returns>
            public bool MoveNext()
            {
                if (index == array.Length) return false;
                ++index;
                return true;
            }

            /// <summary>
            /// Resets this enumerator to one position before the first element in the sequence.
            /// </summary>
            public void Reset()
            {
                index = -1;
            }
            #endregion

            #region Explicit Members
            void IDisposable.Dispose() { }

            object IEnumerator.Current
            {
                get { return Current; }
            }
            #endregion
        }
        #endregion
    }
}
