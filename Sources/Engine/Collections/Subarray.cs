using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// Represents a subset of an array.
    /// </summary>
    /// <remarks>
    /// This structure does the same as <see cref="ArraySegment{T}"/> but has more goodies.
    /// </remarks>
    [Serializable]
    public struct Subarray<T> : IList<T>, IEquatable<Subarray<T>>
    {
        #region Instance
        #region Fields
        private readonly T[] array;
        private readonly int offset;
        private readonly int count;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Subarray{T}"/> representing the whole range of an array.
        /// </summary>
        /// <param name="array">The array to be wrapped.</param>
        public Subarray(T[] array)
        {
            Argument.EnsureNotNull(array, "array");

            this.array = array;
            this.offset = 0;
            this.count = array.Length;
        }

        /// <summary>
        /// Initializes a new <see cref="Subarray{T}"/> from an array and a range.
        /// </summary>
        /// <param name="array">The array to be wrapped.</param>
        /// <param name="offset">The offset of the first value of the range in the array.</param>
        /// <param name="count">The number of values of the range in the array.</param>
        public Subarray(T[] array, int offset, int count)
        {
            Argument.EnsureNotNull(array, "array");
            Argument.EnsureValidRange(offset, count, array.Length, "offset", "count");

            this.array = array;
            this.offset = offset;
            this.count = count;
        }

        /// <summary>
        /// Initializes a new <see cref="Subarray{T}"/> from an array and a range.
        /// </summary>
        /// <param name="array">The array to be wrapped.</param>
        /// <param name="offset">The offset of the first value of the range in the array.</param>
        public Subarray(T[] array, int offset)
        {
            Argument.EnsureNotNull(array, "array");
            if (offset < 0 || offset > array.Length) throw new ArgumentOutOfRangeException("offset");

            this.array = array;
            this.offset = offset;
            this.count = array.Length - offset;
        }

        /// <summary>
        /// Initializes a new <see cref="Subarray{T}"/> from an <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <param name="segment">The <see cref="ArraySegment{T}"/> on which to base this <see cref="Subarray{T}"/>.</param>
        public Subarray(ArraySegment<T> segment)
        {
            Argument.EnsureNotNull(segment.Array, "segment.Array");

            this.array = segment.Array;
            this.offset = segment.Offset;
            this.count = segment.Count;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the array on which this <see cref="Subarray{T}"/> is based.
        /// </summary>
        public T[] Array
        {
            get { return array; }
        }

        /// <summary>
        /// Gets the offset of the first element of this <see cref="Subarray{T}"/>'s range in the base array.
        /// </summary>
        public int Offset
        {
            get { return offset; }
        }

        /// <summary>
        /// Gets the number of elements in this <see cref="Subarray{T}"/>'s range in the base array.
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        private IEnumerable<T> Values
        {
            get
            {
                for (int i = 0; i < Count; ++i)
                    yield return array[Offset + i];
            }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses an element of the base array from the index in this <see cref="Subarray{T}"/>.
        /// </summary>
        /// <param name="index">The index of the element to be accessed.</param>
        /// <returns>The value at that index.</returns>
        public T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return array[offset + index];
            }
            set
            {
                ValidateIndex(index);
                array[offset + index] = value;
            }
        }
        #endregion

        #region Methods
        #region Collection
        /// <summary>
        /// Converts this <see cref="Subarray{T}"/> to an array.
        /// </summary>
        /// <returns>A newly created array containing the elements of this <see cref="Subarray{T}"/></returns>
        public T[] ToArray()
        {
            T[] newArray = new T[count];
            System.Array.Copy(array, offset, newArray, 0, count);
            return newArray;
        }

        /// <summary>
        /// Converts this <see cref="Subarray{T}"/> to its equivalent <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <returns>The resulting <see cref="ArraySegment{T}"/>.</returns>
        public ArraySegment<T> ToSegment()
        {
            return new ArraySegment<T>(array, Offset, Count);
        }

        /// <summary>
        /// Gets an enumerator which enumerates the elements in this <see cref="Subarray{T}"/>.
        /// </summary>
        /// <returns>A new enumerator for this <see cref="Subarray{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Finds the index of the first occurance of an item in this <see cref="Subarray{T}"/>.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns>The index of its first occurance, or -1 if it isn't found.</returns>
        public int IndexOf(T item)
        {
            IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; ++i)
                if (equalityComparer.Equals(array[offset + i], item))
                    return i;
            return -1;
        }

        /// <summary>
        /// Tests if this <see cref="Subarray{T}"/> contains a given item.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns>A value indicating if the item was found.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        /// <summary>
        /// Copies the contents of this <see cref="Subarray{T}"/> to an array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index where to start writing in the destination array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Argument.EnsureNotNull(array, "array");
            System.Array.Copy(this.array, offset, array, arrayIndex, count);
        }

        /// <summary>
        /// Clears all the elements in this <see cref="Subarray{T}"/> to a given value.
        /// </summary>
        /// <param name="value">The value to which the elements are to be cleared.</param>
        public void Clear(T value)
        {
            for (int i = 0; i < count; ++i)
                array[offset + i] = value;
        }

        /// <summary>
        /// Clears all the elements in this <see cref="Subarray{T}"/> to their default value.
        /// </summary>
        public void Clear()
        {
            System.Array.Clear(array, Offset, Count);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
        }
        #endregion

        #region Object Model
        public bool Equals(Subarray<T> other)
        {
            return array == other.array
                && offset == other.offset
                && count == other.count;
        }

        public override bool Equals(object obj)
        {
            return obj is Subarray<T> && Equals((Subarray<T>)obj);
        }

        public override int GetHashCode()
        {
            return array.GetHashCode() ^ offset ^ count;
        }

        public override string ToString()
        {
            return "Subarray [{0}, {1}[".FormatInvariant(offset, offset + count);
        }
        #endregion
        #endregion

        #region Explicit Members
        #region IList<T> Members
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region ICollection<T> Members
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IEnumerable<T> Members
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
        #region Methods
        public static bool Equals(Subarray<T> first, Subarray<T> second)
        {
            return first.Equals(second);
        }
        #endregion

        #region Operators
        public static bool operator==(Subarray<T> lhs, Subarray<T> rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Subarray<T> lhs, Subarray<T> rhs)
        {
            return !Equals(lhs, rhs);
        }

        public static implicit operator Subarray<T>(T[] array)
        {
            return new Subarray<T>(array);
        }

        public static implicit operator Subarray<T>(ArraySegment<T> arraySegment)
        {
            return new Subarray<T>(arraySegment);
        }

        public static implicit operator ArraySegment<T>(Subarray<T> subarray)
        {
            return subarray.ToSegment();
        }
        #endregion
        #endregion
    }
}
