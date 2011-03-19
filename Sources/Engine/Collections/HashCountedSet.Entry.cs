using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Collections
{
    partial class HashCountedSet<T>
    {
        /// <summary>
        /// Represents an entry in the multiset.
        /// </summary>
        [Serializable]
        [DebuggerDisplay("{Item} (x{Count})")]
        public struct Entry
        {
            #region Fields
            /// <summary>
            /// The item value of that entry.
            /// </summary>
            public readonly T Item;

            /// <summary>
            /// The number of occurances of this item.
            /// </summary>
            public readonly int Count;
            #endregion

            #region Constructors
            public Entry(T item, int count)
            {
                this.Item = item;
                this.Count = count;
            }
            #endregion
        }
    }
}
