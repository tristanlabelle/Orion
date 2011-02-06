using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// A bidirectional dictionary which can retrieve both values from keys and keys from values.
    /// The both keys and values must be unique.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public sealed class BiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields
        private readonly IEqualityComparer<TKey> keyEqualityComparer;
        private readonly IEqualityComparer<TValue> valueEqualityComparer;
        private readonly Dictionary<TKey, TValue> keysToValues;
        private readonly Dictionary<TValue, TKey> valuesToKeys;
        #endregion

        #region Constructors
        public BiDictionary()
            : this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default) { }

        public BiDictionary(IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer)
        {
            this.keyEqualityComparer = keyEqualityComparer ?? EqualityComparer<TKey>.Default;
            this.valueEqualityComparer = valueEqualityComparer ?? EqualityComparer<TValue>.Default;
            this.keysToValues = new Dictionary<TKey, TValue>(keyEqualityComparer);
            this.valuesToKeys = new Dictionary<TValue, TKey>(valueEqualityComparer);
        }

        public BiDictionary(int capacity)
            : this(capacity, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default) { }

        public BiDictionary(int capacity, IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer)
        {
            this.keyEqualityComparer = keyEqualityComparer ?? EqualityComparer<TKey>.Default;
            this.valueEqualityComparer = valueEqualityComparer ?? EqualityComparer<TValue>.Default;
            this.keysToValues = new Dictionary<TKey, TValue>(capacity, keyEqualityComparer);
            this.valuesToKeys = new Dictionary<TValue, TKey>(capacity, valueEqualityComparer);
        }

        public BiDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries)
            : this(entries, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default) { }

        public BiDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries,
            IEqualityComparer<TKey> keyEqualityComparer, IEqualityComparer<TValue> valueEqualityComparer)
        {
            Argument.EnsureNotNull(entries, "entries");

            this.keyEqualityComparer = keyEqualityComparer ?? EqualityComparer<TKey>.Default;
            this.valueEqualityComparer = valueEqualityComparer ?? EqualityComparer<TValue>.Default;
            this.keysToValues = new Dictionary<TKey, TValue>();
            this.valuesToKeys = new Dictionary<TValue, TKey>();

            foreach (var entry in entries)
            {
                keysToValues.Add(entry.Key, entry.Value);
                valuesToKeys.Add(entry.Value, entry.Key);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of keys and values this dictionary contains.
        /// </summary>
        public int Count
        {
            get { return keysToValues.Count; }
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get { return keysToValues.Keys; }
        }

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        public Dictionary<TValue, TKey>.KeyCollection Values
        {
            get { return valuesToKeys.Keys; }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses the value associated with a given key.
        /// When setting, the value being set must be unique.
        /// </summary>
        /// <param name="key">The key to be tested.</param>
        /// <returns>The value assigned to that key.</returns>
        public TValue this[TKey key]
        {
            get { return GetValue(key); }
            set { SetValue(key, value); }
        }
        #endregion

        #region Methods
        #region Containment
        /// <summary>
        /// Tests if a key exists in this dictionary.
        /// </summary>
        /// <param name="key">The key to be found.</param>
        /// <returns><c>True</c> if the key exists in this dictionary, <c>false</c> if it does not.</returns>
        public bool ContainsKey(TKey key)
        {
            return keysToValues.ContainsKey(key);
        }

        /// <summary>
        /// Tests if a value exists in this dictionary.
        /// </summary>
        /// <param name="value">The value to be found.</param>
        /// <returns><c>True</c> if the value exists in this dictionary, <c>false</c> if it does not.</returns>
        public bool ContainsValue(TValue value)
        {
            return valuesToKeys.ContainsKey(value);
        }
        #endregion

        #region Retrieval
        /// <summary>
        /// Retrieves the value associated with a given key.
        /// </summary>
        /// <param name="key">The key to be found.</param>
        /// <returns>The associated value.</returns>
        /// <exception cref="KeyNotFoundException">There is no value associated with that key.</exception>
        public TValue GetValue(TKey key)
        {
            return keysToValues[key];
        }

        /// <summary>
        /// Retrieves the key associated with a given value.
        /// </summary>
        /// <param name="value">The value to be found.</param>
        /// <returns>The associated key.</returns>
        /// <exception cref="KeyNotFoundException">There is no value associated with that value.</exception>
        public TKey GetKey(TValue value)
        {
            return valuesToKeys[value];
        }

        /// <summary>
        /// Attempts to retrieve the value associated with a given key.
        /// </summary>
        /// <param name="key">The key to be found.</param>
        /// <param name="value">
        /// If the operation succeeds, outputs the value associated with that key.
        /// Otherwise, outputs the default value for <typeparamref name="TValue"/>.
        /// </param>
        /// <returns>
        /// <c>True</c> if the key was found and its value retrieved, <c>false</c> if the key was not found.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return keysToValues.TryGetValue(key, out value);
        }

        /// <summary>
        /// Attempts to retrieve the key associated with a given value.
        /// </summary>
        /// <param name="value">The value to be found.</param>
        /// <param name="key">
        /// If the operation succeeds, outputs the key associated with that value.
        /// Otherwise, outputs the default value for <typeparamref name="TKey"/>.
        /// </param>
        /// <returns>
        /// <c>True</c> if the value was found and its key retrieved, <c>false</c> if the value was not found.
        /// </returns>
        public bool TryGetKey(TValue value, out TKey key)
        {
            return valuesToKeys.TryGetValue(value, out key);
        }

        /// <summary>
        /// Attempts to retreive a value by its key.
        /// </summary>
        /// <param name="key">The key for which the associated value should be found.</param>
        /// <param name="defaultValue">The value to return if the key is not found.</param>
        /// <returns>
        /// The value associated with <paramref name="key"/>, or <paramref name="defaultValue"/> if the key is not found.
        /// </returns>
        public TValue GetValueOrDefault(TKey key, TValue defaultValue)
        {
            TValue value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Attempts to retreive a value by its key.
        /// </summary>
        /// <param name="key">The key for which the associated value should be found.</param>
        /// <returns>
        /// The value associated with <paramref name="key"/>, or the default value value if the key is not found.
        /// </returns>
        public TValue GetValueOrDefault(TKey key)
        {
            return GetValueOrDefault(key, default(TValue));
        }

        /// <summary>
        /// Attempts to retreive a key by its value.
        /// </summary>
        /// <param name="value">The value for which the associated key should be found.</param>
        /// <param name="defaultKey">The key to return if the value is not found.</param>
        /// <returns>
        /// The key associated with <paramref name="value"/>, or <paramref name="defaultKey"/> if the value is not found.
        /// </returns>
        public TKey GetKeyOrDefault(TValue value, TKey defaultKey)
        {
            TKey key;
            return TryGetKey(value, out key) ? key : defaultKey;
        }

        /// <summary>
        /// Attempts to retreive a key by its value.
        /// </summary>
        /// <param name="value">The value for which the associated key should be found.</param>
        /// <returns>
        /// The key associated with <paramref name="value"/>, or the default key value if the value is not found.
        /// </returns>
        public TKey GetKeyOrDefault(TValue value)
        {
            return GetKeyOrDefault(value, default(TKey));
        }
        #endregion

        #region Addition
        /// <summary>
        /// Adds a key-value pair to this dictionary.
        /// </summary>
        /// <param name="key">The key to be added.</param>
        /// <param name="value">The value to be added.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if either the key or the value are already in use.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            keysToValues.Add(key, value);
            try { valuesToKeys.Add(value, key); }
            catch (ArgumentException)
            {
                keysToValues.Remove(key);
                throw;
            }
        }
        #endregion

        #region Removal
        /// <summary>
        /// Attempts to remove an entry from this dictionary by its key.
        /// </summary>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns>
        /// <c>True</c> if the key was found and the entry removed, <c>false</c> if the key was not found.
        /// </returns>
        public bool RemoveByKey(TKey key)
        {
            TValue value;
            if (!keysToValues.TryGetValue(key, out value))
                return false;

            bool wasKeyRemoved = keysToValues.Remove(key);
            bool wasValueRemoved = valuesToKeys.Remove(value);
            Debug.Assert(wasKeyRemoved && wasValueRemoved);

            return true;
        }

        /// <summary>
        /// Attempts to remove an entry from this dictionary by its value.
        /// </summary>
        /// <param name="value">The value of the entry to be removed.</param>
        /// <returns>
        /// <c>True</c> if the value was found and the entry removed, <c>false</c> if the value was not found.
        /// </returns>
        public bool RemoveByValue(TValue value)
        {
            TKey key;
            if (!valuesToKeys.TryGetValue(value, out key))
                return false;

            bool wasValueRemoved = valuesToKeys.Remove(value);
            bool wasKeyRemoved = keysToValues.Remove(key);
            Debug.Assert(wasValueRemoved && wasKeyRemoved);

            return true;
        }

        /// <summary>
        /// Removes all entries from this dictionary.
        /// </summary>
        public void Clear()
        {
            keysToValues.Clear();
            valuesToKeys.Clear();
        }
        #endregion

        #region Changing
        /// <summary>
        /// Adds or changes the value associated with a key. The new value must be unique.
        /// </summary>
        /// <param name="key">The key for which the value is to be set.</param>
        /// <param name="value">The new value for that key.</param>
        public void SetValue(TKey key, TValue value)
        {
            // Check value usage.
            TKey clashingKey;
            if (valuesToKeys.TryGetValue(value, out clashingKey))
            {
                if (keyEqualityComparer.Equals(key, clashingKey))
                {
                    // No-op, the key already has this value.
                    return;
                }

                string message = "Cannot assign value {0} to key {1}, that value is already in use by key {2}."
                    .FormatInvariant(value, key, clashingKey);
                throw new ArgumentException(message);
            }

            valuesToKeys.Add(value, key);
            keysToValues[key] = value;
        }


        /// <summary>
        /// Adds or changes the key associated with a value. The new key must be unique.
        /// </summary>
        /// <param name="value">The value for which the key is to be set.</param>
        /// <param name="key">The new key for that value.</param>
        public void SetKey(TValue value, TKey key)
        {
            // Check key usage.
            TValue clashingValue;
            if (keysToValues.TryGetValue(key, out clashingValue))
            {
                if (valueEqualityComparer.Equals(value, clashingValue))
                {
                    // No-op, the value already has this key.
                    return;
                }

                string message = "Cannot assign key {0} to value {1}, that key is already in use by value {2}."
                    .FormatInvariant(key, value, clashingValue);
                throw new ArgumentException(message);
            }

            keysToValues.Add(key, value);
            valuesToKeys[value] = key;
        }
        #endregion

        #region Enumeration
        /// <summary>
        /// Gets an enumerator which iterates over the entries of this dictionary as key-value pairs.
        /// </summary>
        /// <returns>A new dictionary enumerator.</returns>
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return keysToValues.GetEnumerator();
        }
        #endregion
        #endregion

        #region Explicit Members
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return RemoveByKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return Keys; }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return Values; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)keysToValues).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)keysToValues).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)keysToValues).Remove(item);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
