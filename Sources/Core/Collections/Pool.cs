using System;
using System.Collections.Generic;

namespace Orion.Collections
{
    /// <summary>
    /// Provides a pool of objects to facilitate reuse.
    /// </summary>
    /// <typeparam name="T">The type of the objects in this pool.</typeparam>
    [Serializable]
    public sealed class Pool<T> where T : class
    {
        #region Fields
        private static readonly Func<T> DefaultFactory = Activator.CreateInstance<T>;

        private readonly Func<T> factory;
        private readonly Stack<T> items = new Stack<T>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Pool{T}"/> which uses the
        /// <typeparamref name="T"/>'s default constructor to create
        /// new instances.
        /// </summary>
        public Pool()
        {
            factory = DefaultFactory;
        }

        /// <summary>
        /// Initializes a new <see cref="Pool{T}"/> from a delegate
        /// that can create new instances of pooled objects.
        /// </summary>
        /// <param name="factory">The factory method to be used to instantiate new objects.</param>
        public Pool(Func<T> factory)
        {
            Argument.EnsureNotNull(factory, "factory");
            this.factory = factory;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of pooled items in this <see cref="Pool{T}"/>.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new item to this <see cref="Pool{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added. It is assumed to be absent.</param>
        public void Add(T item)
        {
            Argument.EnsureNotNull(item, "item");
            items.Push(item);
        }

        /// <summary>
        /// Gets a <typeparamref name="T"/> from the pooled items,
        /// or creates one if the pool is empty.
        /// </summary>
        /// <returns>The <typeparamref name="T"/> that was retrieved or created.</returns>
        public T Get()
        {
            if (items.Count > 0) return items.Pop();
            return factory();
        }
        #endregion
    }
}
