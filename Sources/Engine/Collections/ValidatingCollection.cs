using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// A collection which validates items added to it using a predicate.
    /// </summary>
    [Serializable]
    public sealed class ValidatingCollection<T> : ICollection<T>
    {
        #region Fields
        private readonly ICollection<T> items;
        private readonly Func<T, bool> predicate;
        #endregion

        #region Constructors
        public ValidatingCollection(ICollection<T> items, Func<T, bool> predicate)
        {
            Argument.EnsureNotNull(items, "items");
            Argument.EnsureEqual(items.IsReadOnly, false, "items.IsReadOnly");
            Argument.EnsureNotNull(predicate, "predicate");

            this.items = items;
            this.predicate = predicate;
        }

        public ValidatingCollection(Func<T, bool> predicate)
            : this(new List<T>(), predicate)
        { }
        #endregion

        #region Properties
        public int Count
        {
            get { return items.Count; }
        }
        #endregion

        #region Methods
        public void Add(T item)
        {
            if (!predicate(item))
            {
                throw new ArgumentException("Cannot add invalid item {0} to collection."
                    .FormatInvariant(item));
            }

            items.Add(item);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        #endregion

        #region Explicit Members
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
