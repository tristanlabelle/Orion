using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Collections
{
    /// <summary>
    /// A collection in which elements can be quickly added with minimum reallocations,
    /// but that does not support removing.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    [Serializable]
    internal sealed class ChunkList<T> : ICollection<T>
    {
        private sealed class Node
        {
            public readonly T[] Buffer;
            public Node Next;

            public Node(int count)
            {
                Buffer = new T[count];
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            #region Fields
            private readonly ChunkList<T> list;
            private Node currentNode;
            private int indexInNode;
            #endregion

            #region Constructors
            internal Enumerator(ChunkList<T> list)
            {
                this.list = list;
                this.currentNode = list.firstNode;
                this.indexInNode = -1;
            }
            #endregion

            #region Properties
            public T Current
            {
                get { return currentNode.Buffer[indexInNode]; }
            }
            #endregion

            #region Methods
            public bool MoveNext()
            {
                if (currentNode == list.lastNode && indexInNode >= list.countInLastNode - 1)
                {
                    return false;
                }

                if (indexInNode == currentNode.Buffer.Length - 1)
                {
                    indexInNode = 0;
                    currentNode = currentNode.Next;
                }
                else
                {
                    ++indexInNode;
                }

                return true;
            }

            public void Reset()
            {
                this.currentNode = list.firstNode;
                this.indexInNode = -1;
            }
            #endregion

            #region Explicit Members
            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            void IDisposable.Dispose() { }
            #endregion
        }

        #region Fields
        private readonly Node firstNode;
        private Node lastNode;
        private int count;
        private int countInLastNode;
        #endregion

        #region Constructors
        public ChunkList()
        {
            firstNode = new Node(8);
            lastNode = firstNode;
        }

        public ChunkList(IEnumerable<T> items)
            : this()
        {
            Argument.EnsureNotNull(items, "items");

            foreach (T item in items) Add(item);
        }
        #endregion

        #region Properties
        public int Count
        {
            get { return count; }
        }
        #endregion

        #region Methods
        public void Add(T item)
        {
            if (countInLastNode == lastNode.Buffer.Length)
            {
                if (lastNode.Next == null) lastNode.Next = new Node(lastNode.Buffer.Length * 2);
                lastNode = lastNode.Next;
                countInLastNode = 0;
            }

            lastNode.Buffer[countInLastNode] = item;
            countInLastNode++;
            count++;
        }

        /// <summary>
        /// Removes all items from this collection.
        /// </summary>
        public void Clear()
        {
            Node node = firstNode;
            while (true)
            {
                if (node == lastNode)
                {
                    Array.Clear(node.Buffer, 0, countInLastNode);
                    countInLastNode = 0;
                    break;
                }

                Array.Clear(node.Buffer, 0, node.Buffer.Length);
                node = node.Next;
            }

            lastNode = firstNode;
            count = 0;
        }

        public bool Contains(T item)
        {
            return Enumerable.Contains(this, item);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Node node = firstNode;
            int bufferIndex = 0;
            int index = 0;
            while (index < count)
            {
                if (bufferIndex == node.Buffer.Length)
                {
                    bufferIndex = 0;
                    node = node.Next;
                }

                array[arrayIndex + index] = node.Buffer[bufferIndex];
                bufferIndex++;
                index++;
            }
        }
        #endregion

        #region Explicit Members
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
