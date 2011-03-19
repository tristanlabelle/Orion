using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// A hash-based set that can contain multiple instances of each item.
    /// A counted set maintains an item-count entry for each item. When a new item
    /// that compares equal to a previous one is added, the item is discarded and the
    /// entry count of the original item is increased.
    /// </summary>
    /// <typeparam name="T">The type of the items in the set.</typeparam>
    [Serializable]
    [DebuggerDisplay("Count = {Count}, DistinctCount = {DistinctCount}")]
    public sealed partial class HashCountedSet<T> : ICollection<T>
    {
        #region ChangeType Enumeration
        private enum ChangeType
        {
            Relative,
            Absolute
        }
        #endregion

        #region Fields
        private static readonly int firstHashingPrimeIndex = HashingPrimes.IndexOfNext(16);
        private static readonly float maximumLoadFactor = 0.75f;

        private readonly IEqualityComparer<T> equalityComparer;
        private InternalEntry[] entries = new InternalEntry[64];
        private int nextFreeEntryIndex = -1;
        private int initializedEntryCount;
        private int primeIndex = firstHashingPrimeIndex;
        private int[] buckets; // 1-based indices into entries, 0 means none
        private int count;
        private int entryCount;
        private int version;
        #endregion

        #region Constructors
        public HashCountedSet()
        {
            equalityComparer = EqualityComparer<T>.Default;
            buckets = new int[HashingPrimes.At(primeIndex)];
        }

        public HashCountedSet(IEqualityComparer<T> equalityComparer)
        {
            Argument.EnsureNotNull(equalityComparer, "equalityComparer");

            this.equalityComparer = equalityComparer;
            buckets = new int[HashingPrimes.At(primeIndex)];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of items in this multiset, counting all occurances.
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Gets the number of distinct items in this multiset.
        /// </summary>
        public int DistinctCount
        {
            get { return entryCount; }
        }

        /// <summary>
        /// Gets the entries in this multiset.
        /// </summary>
        public IEnumerable<Entry> Entries
        {
            get
            {
                int initialVersion = version;
                for (int i = 0; i < entries.Length; ++i)
                {
                    InternalEntry entry = entries[i];
                    if (entry.Count == 0) continue;

                    CheckVersion(initialVersion);
                    yield return new Entry(entry.Item, entry.Count);
                }
            }
        }
        #endregion

        #region Methods
        #region Retrieving
        /// <summary>
        /// Gets the number of occurances of a given item in this multiset.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns>The number of times the item occurs.</returns>
        public int GetCount(T item)
        {
            return Change(item, ChangeType.Relative, 0);
        }

        /// <summary>
        /// Gets a value indicating if a given item is present in this multiset.
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <returns><c>True</c> if that item can be found in the multiset, <c>false</c> if not.</returns>
        public bool Contains(T item)
        {
            return Change(item, ChangeType.Relative, 0) > 0;
        }

        /// <summary>
        /// Gets the number of distinct elements in this multiset.
        /// </summary>
        /// <returns>A new enumerator over this set's distinct elements.</returns>
        public IEnumerable<T> Distinct()
        {
            int initialVersion = version;
            for (int i = 0; i < entries.Length; ++i)
            {
                if (!entries[i].IsUsed) continue;
                CheckVersion(initialVersion);

                yield return entries[i].Item;
            }
        }

        /// <summary>
        /// Gets an enumerator that enumerates every occurance of every item in this set.
        /// </summary>
        /// <returns>A new enumerator over this set's element's occurances.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            int initialVersion = version;
            for (int i = 0; i < entries.Length; ++i)
            {
                InternalEntry entry = entries[i];
                for (int j = 0; j < entry.Count; ++j)
                {
                    CheckVersion(initialVersion);
                    yield return entry.Item;
                }
            }
        }
        #endregion

        #region Setting
        /// <summary>
        /// Sets the number of occurances of an item in this multiset.
        /// </summary>
        /// <param name="item">The item which's number of occurances is to be set.</param>
        /// <param name="count">The new number of occurances of that item.</param>
        /// <returns>The previous number of occurances of that item.</returns>
        public int Set(T item, int count)
        {
            return Change(item, ChangeType.Absolute, count);
        }
        #endregion

        #region Adding
        /// <summary>
        /// Adds an occurance of an item to this multiset.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>The new number of occurances of this item.</returns>
        public int Add(T item)
        {
            return Change(item, ChangeType.Relative, 1) + 1;
        }

        /// <summary>
        /// Adds a given number of occurances of an item to this multiset.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <param name="count">The number of occurances to be added.</param>
        /// <returns>The new number of occurances of this item.</returns>
        public int Add(T item, int count)
        {
            return Math.Max(Change(item, ChangeType.Relative, count) + count, 0);
        }
        #endregion

        #region Removing
        /// <summary>
        /// Removes an occurance of an item from this multiset.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>The new number of occurances of this item.</returns>
        public int Remove(T item)
        {
            return Math.Max(Change(item, ChangeType.Relative, -1), 0);
        }

        /// <summary>
        /// Removes a given number of occurances of an item from this multiset.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="count">The number of occurances to be removed.</param>
        /// <returns>The new number of occurances of this item.</returns>
        public int Remove(T item, int count)
        {
            return Math.Max(Change(item, ChangeType.Relative, -count) - count, 0);
        }

        /// <summary>
        /// Removes all occurances of an item from this multiset.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>The previous number of occurances of this item.</returns>
        public int RemoveAll(T item)
        {
            return Change(item, ChangeType.Absolute, 0);
        }

        /// <summary>
        /// Removes all items from this multiset.
        /// </summary>
        public void Clear()
        {
            Array.Clear(buckets, 0, buckets.Length);
            Array.Clear(entries, 0, entries.Length);

            nextFreeEntryIndex = 0;
            initializedEntryCount = 0;
            entryCount = 0;
            count = 0;
            ++version;
        }
        #endregion

        /// <summary>
        /// Applies a change to the number of occurances of an item.
        /// </summary>
        /// <param name="item">The item which's number of occurances is to be changed.</param>
        /// <param name="type">The type of change to be applied.</param>
        /// <param name="amount">
        /// The amount of change.
        /// If <paramref name="type"/> is <see cref="ChangeType.Relative"/>, this is the number of occurances to add.
        /// If <paramref name="type"/> is <see cref="ChangeType.Absolute"/>, this is the number of occurances to set.
        /// </param>
        /// <returns>The previous number of occurances.</returns>
        private int Change(T item, ChangeType type, int amount)
        {
            float loadFactor = entryCount / (float)buckets.Length;
            if (loadFactor > maximumLoadFactor) IncreaseBucketCount();

            uint hashCode = unchecked((uint)equalityComparer.GetHashCode(item));
            int bucketIndex = unchecked((int)(hashCode % buckets.Length));

            int entryIndex = buckets[bucketIndex] - 1;
            int previousEntryIndex = -1;
            while (entryIndex != -1)
            {
                InternalEntry entry = entries[entryIndex];
                if (entry.HashCode == hashCode && equalityComparer.Equals(entry.Item, item))
                {
                    int oldCount = entry.Count;
                    if (type == ChangeType.Relative)
                    {
                        if (amount == 0) return entry.Count;
                        if (-amount > entry.Count) amount = -entry.Count;

                        entry.Count += amount;
                        count += amount;
                    }
                    else
                    {
                        if (amount == entry.Count) return entry.Count;

                        count = count - entry.Count + amount;
                        entry.Count = amount;
                    }

                    if (entry.Count == 0)
                    {
                        // Remove the entry
                        if (previousEntryIndex != -1)
                            entries[previousEntryIndex].NextIndex = entry.NextIndex;
                        else
                            buckets[bucketIndex] = entry.NextIndex + 1;

                        entries[entryIndex].NextIndex = nextFreeEntryIndex;
                        entries[entryIndex].Count = 0;
                        nextFreeEntryIndex = entryIndex;
                        --entryCount;
                    }
                    else
                    {
                        // Update the entry
                        entries[entryIndex].Count = entry.Count;
                    }

                    ++version;

                    return oldCount;
                }

                previousEntryIndex = entryIndex;
                entryIndex = entry.NextIndex;
            }

            if (amount <= 0) return 0;

            entryIndex = nextFreeEntryIndex;
            if (entryIndex == -1)
            {
                if (initializedEntryCount == entries.Length)
                    Array.Resize(ref entries, entries.Length * 2);

                entryIndex = initializedEntryCount;
                ++initializedEntryCount;
            }
            else
            {
                nextFreeEntryIndex = entries[entryIndex].NextIndex;
            }

            entries[entryIndex] = new InternalEntry(item, amount, hashCode, buckets[bucketIndex] - 1);
            buckets[bucketIndex] = entryIndex + 1;

            count += amount;
            ++entryCount;
            ++version;

            return 0;
        }

        private void IncreaseBucketCount()
        {
            do
            {
                ++primeIndex;
            } while (HashingPrimes.At(primeIndex) < buckets.Length * 2);

            buckets = new int[HashingPrimes.At(primeIndex)];

            for (int i = 0; i < entries.Length; ++i)
            {
                if (entries[i].Count == 0) continue;

                int bucketIndex = unchecked((int)(entries[i].HashCode % buckets.Length));

                int nextEntryIndex = buckets[bucketIndex] - 1;
                entries[i].NextIndex = nextEntryIndex;
                buckets[bucketIndex] = i + 1;
            }
        }

        private void CheckVersion(int version)
        {
            if (version != this.version) throw new InvalidOperationException("The multiset has been modified while it was being enumerated.");
        }
        #endregion

        #region Explicit Members
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            return Change(item, ChangeType.Relative, -1) > 0;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
