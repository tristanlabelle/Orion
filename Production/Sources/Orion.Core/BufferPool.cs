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
        private static readonly Func<int, T[]> DefaultAllocator = (length => new T[length]);

        /// <remarks>
        /// Ideally those should be sorted but it shouldn't be a big deal if they're not.
        /// </remarks>
        private readonly List<T[]> buffers = new List<T[]>();
        private readonly Func<int, T[]> allocator;
        #endregion

        #region Constructors
        public BufferPool(Func<int, T[]> allocator)
        {
            Argument.EnsureNotNull(allocator, "allocator");
            this.allocator = allocator;
        }

        public BufferPool() : this(DefaultAllocator) { }
        #endregion

        #region Methods
        /// <summary>
        /// Allocates a new buffer using this pool's internal allocator.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the buffer to allocate.</param>
        /// <returns>The buffer that was allocated.</returns>
        public T[] Allocate(int minimumLength)
        {
            Argument.EnsurePositive(minimumLength, "length");

            T[] buffer = allocator(minimumLength);
            if (buffer == null || buffer.Length < minimumLength)
                throw new InvalidOperationException("Buffer allocator did not return a valid buffer.");

            return buffer;
        }
        
        /// <summary>
        /// Gets and removes a buffer from this pool from a minimum length,
        /// or creates one if none are big enough.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the buffer that is needed.</param>
        /// <returns>A buffer with at least the specified length.</returns>
        public T[] Get(int minimumLength)
        {
            Argument.EnsurePositive(minimumLength, "length");

            int bestFitBufferIndex = GetBestFitPooledBufferIndex(minimumLength);
            if (bestFitBufferIndex != -1)
            {
                T[] bestFitBuffer = buffers[bestFitBufferIndex];
                buffers.RemoveAt(bestFitBufferIndex);
                return bestFitBuffer;
            }

            return Allocate(minimumLength);
        }

        private int GetBestFitPooledBufferIndex(int minimumLength)
        {
            int index = -1;
            for (int i = 0; i < buffers.Count; ++i)
            {
                if (buffers[i].Length < minimumLength) continue;

                if (index == -1 || buffers[i].Length < buffers[index].Length)
                {
                    index = i;

                    if (buffers[index].Length == minimumLength)
                        break;
                }
            }
            return index;
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
