using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Orion
{
    /// <summary>
    /// A list of items which use a buffer pool instead of allocating its own buffers.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class PooledList<T> : IList<T>
    {
        #region Fields
        private readonly BufferPool<T> bufferPool;
        private readonly IEqualityComparer<T> equalityComparer;
        private T[] buffer;
        private int count;
        #endregion

        #region Constructors
        public PooledList(BufferPool<T> bufferPool, IEqualityComparer<T> equalityComparer)
        {
            Argument.EnsureNotNull(bufferPool, "bufferPool");
            Argument.EnsureNotNull(equalityComparer, "equalityComparer");
            this.bufferPool = bufferPool;
            this.equalityComparer = equalityComparer;
        }

        public PooledList(BufferPool<T> bufferPool)
            : this(bufferPool, EqualityComparer<T>.Default) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of items in this list.
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses an item from this list by its index.
        /// </summary>
        /// <param name="index">The index of the item to be retrieved.</param>
        /// <returns>The item at that index.</returns>
        public T this[int index]
        {
            get { return buffer[index]; }
            set { buffer[index] = value; }
        }
        #endregion

        #region Methods
        #region Queries
        /// <summary>
        /// Tests if an item is present in this list.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns>A value indicating if that item is present in this list.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// Gets the index of the first occurance of an item in this list.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns>The index of item, or <c>-1</c> if it wasn't found.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < count; ++i)
                if (equalityComparer.Equals(buffer[i], item))
                    return i;
            return -1;
        }
        #endregion

        #region Adding
        /// <summary>
        /// Adds an item to this list.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            if (buffer == null)
            {
                buffer = bufferPool.Get(1);
            }
            else if (count == buffer.Length)
            {
                T[] newUnitBuffer = bufferPool.Get(buffer.Length + 1);
                Array.Copy(buffer, newUnitBuffer, buffer.Length);
                bufferPool.Add(buffer);
                buffer = newUnitBuffer;
            }

            buffer[count] = item;
            ++count;
        }
        #endregion

        #region Removing
        /// <summary>
        /// Removes an item from this zone.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns><c>True</c> if an item was removed, <c>false</c> if it wasn't found.</returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes an item at an index in this list.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        public void RemoveAt(int index)
        {
            if (index < count - 1) buffer[index] = buffer[count - 1];
            buffer[count - 1] = default(T);
            --count;

            if (count == 0)
            {
                // Return our buffer to the pool as we do not need it anymore.
                bufferPool.Add(buffer);
                buffer = null;
            }
            else if (count <= buffer.Length / 3)
            {
                // The list is getting quite empty, attempt to get a smaller
                // buffer so there's less wasted space and other pool clients
                // can benefit from our big buffer.
                T[] newUnitBuffer = bufferPool.GetPooled(count);
                if (newUnitBuffer != null && newUnitBuffer.Length < buffer.Length)
                {
                    Array.Copy(buffer, newUnitBuffer, count);
                    bufferPool.Add(buffer);
                    buffer = newUnitBuffer;
                }
            }
        }

        /// <summary>
        /// Removes all items from this list.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < count; ++i)
                buffer[i] = default(T);

            if (buffer != null)
            {
                count = 0;
                bufferPool.Add(buffer);
                buffer = null;
            }
        }
        #endregion
        #endregion

        #region Explicit Members
        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (count == 0) return;
            Array.Copy(buffer, 0, array, arrayIndex, count);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // Should be implemented, I'm just too lazy.
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
        #endregion
    }
}
