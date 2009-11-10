using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Provides a pool of arrays which facilitates their reuse as buffers.
    /// </summary>
    /// <typeparam name="T">The type of item in the buffers.</typeparam>
    [Serializable]
    public sealed class BufferPool<T>
    {
        #region Fields
        private const int DefaultMinimumAllocationLength = 16;

        /// <remarks>
        /// Ideally those should be sorted but it shouldn't be a big deal if they're not.
        /// </remarks>
        private readonly List<T[]> buffers = new List<T[]>();
        private readonly int minimumAllocationLength;
        #endregion

        #region Constructors
        public BufferPool(int minimumAllocationLength)
        {
            Argument.EnsurePositive(minimumAllocationLength, "minimumAllocationLength");
            this.minimumAllocationLength = minimumAllocationLength;
        }

        public BufferPool() : this(DefaultMinimumAllocationLength) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the minimum length of buffers allocated by this pool.
        /// </summary>
        public int MinimumAllocationLength
        {
            get { return minimumAllocationLength; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets and removes a buffer from this pool from a minimum length,
        /// or creates one if none are big enough.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the buffer that is needed.</param>
        /// <returns>A buffer with at least the specified length.</returns>
        public T[] Get(int minimumLength)
        {
            Argument.EnsurePositive(minimumLength, "length");

            for (int i = 0; i < buffers.Count; ++i)
            {
                T[] buffer = buffers[i];
                if (buffer.Length >= minimumLength)
                {
                    buffers.RemoveAt(i);
                    return buffer;
                }
            }

            int bufferLength = Math.Max(minimumLength, minimumAllocationLength);
            return new T[bufferLength];
        }

        /// <summary>
        /// Adds a buffer to this pool so it can be reused.
        /// </summary>
        /// <param name="buffer">The buffer to be added.</param>
        /// <remarks>
        /// Buffers inserted here need not to have a length of <see cref="MinimumAllocationLength"/>.
        /// </remarks>
        public void Add(T[] buffer)
        {
            Argument.EnsureNotNull(buffer, "buffer");

            if (!buffers.Contains(buffer))
                buffers.Add(buffer);
        }
        #endregion
    }
}
