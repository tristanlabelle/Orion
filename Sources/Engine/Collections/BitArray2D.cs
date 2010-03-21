using System;
using System.Collections;
using System.Collections.Generic;

namespace Orion.Engine.Collections
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
        private readonly int width;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance with the specified dimensions and a default value for the bits.
        /// </summary>
        /// <param name="width">The width of the array.</param>
        /// <param name="height">The height of the array.</param>
        /// <param name="defaultValue">The default value of the bits in this array.</param>
        public BitArray2D(int width, int height, bool defaultValue)
        {
            Argument.EnsurePositive(width, "width");
            Argument.EnsurePositive(height, "height");

            this.bits = new BitArray(width * height, defaultValue);
            this.width = width;
        }

        /// <summary>
        /// Initializes a new instance with the specified dimensions.
        /// </summary>
        /// <param name="width">The width of the array.</param>
        /// <param name="height">The height of the array.</param>
        public BitArray2D(int width, int height)
            : this(width, height, false) { }

        public BitArray2D(Size size, bool defaultValue)
            : this(size.Width, size.Height, defaultValue) { }

        public BitArray2D(Size size)
            : this(size, false) { }

        /// <summary>
        /// Initializes a new instance from an array of bits to be copied.
        /// </summary>
        /// <param name="bits">The bits to be copied.</param>
        /// <param name="width">The width of the array.</param>
        /// <param name="height">The height of the array.</param>
        public BitArray2D(BitArray bits, int width, int height)
        {
            Argument.EnsureNotNull(bits, "bits");
            Argument.EnsurePositive(width, "width");
            Argument.EnsurePositive(height, "height");

            if (bits.Length != width * height)
                throw new ArgumentException("Width and height do not match bit array size.");

            this.bits = (BitArray)bits.Clone();
            this.width = width;
        }

        /// <summary>
        /// Initializes a new instance from an array of bits to be copied.
        /// </summary>
        /// <param name="bits">The bits to be copied.</param>
        /// <param name="width">The width of the array.</param>
        /// <param name="height">The height of width array.</param>
        public BitArray2D(bool[] bits, int width, int height)
        {
            Argument.EnsureNotNull(bits, "bits");
            Argument.EnsurePositive(width, "width");
            Argument.EnsurePositive(height, "height");

            if (bits.Length != width * height)
                throw new ArgumentException("Width and height do not match bit array size.");

            this.bits = new BitArray(bits);
            this.width = width;
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
        /// Gets the width of this 2D array.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height of this 2D array.
        /// </summary>
        public int Height
        {
            get { return bits.Count / width; }
        }

        /// <summary>
        /// Gets the Size of this 2D array.
        /// </summary>
        public Size Size
        {
            get { return new Size(width, Height); }
        }

        /// <summary>
        /// Gets the area of this bit array, in bits.
        /// </summary>
        public int Area
        {
            get { return bits.Length; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses a bit at a given location in this array.
        /// </summary>
        /// <param name="x">The x coordinate of the bit.</param>
        /// <param name="y">The y coordinate of the bit.</param>
        /// <returns>The value of the bit at that location.</returns>
        public bool this[int x, int y]
        {
            get { return bits[GetIndex(x, y)]; }
            set { bits[GetIndex(x, y)] = value; }
        }

        /// <summary>
        /// Accesses a bit at a given location in this array.
        /// </summary>
        /// <param name="point">The coordinates of the bit.</param>
        /// <returns>The value of the bit at that location.</returns>
        public bool this[Point point]
        {
            get { return this[point.X, point.Y]; }
            set { this[point.X, point.Y] = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clones this 2D bit array.
        /// </summary>
        /// <returns>The bit array to be cloned.</returns>
        public BitArray2D Clone()
        {
            return new BitArray2D(bits, Width, Height);
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
            return "{0}x{1} bits".FormatInvariant(width, Height);
        }

        private int GetIndex(int x, int y)
        {
            return y * width + x;
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
                if (index == array.Area) return false;
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
