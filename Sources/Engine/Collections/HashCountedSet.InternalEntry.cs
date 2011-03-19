using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Collections
{
    partial class HashCountedSet<T>
    {
        private struct InternalEntry
        {
            #region Fields
            public T Item;
            public int Count;
            public uint HashCode;
            public int NextIndex;
            #endregion

            #region Constructors
            public InternalEntry(T item, int count, uint hashCode, int nextIndex)
            {
                this.Item = item;
                this.Count = count;
                this.HashCode = hashCode;
                this.NextIndex = nextIndex;
            }

            public InternalEntry(T item, int count, uint hashCode)
                : this(item, count, hashCode, -1) { }
            #endregion

            #region Properties
            public bool IsUsed
            {
                get { return Count > 0; }
            }
            #endregion
        }
    }
}
