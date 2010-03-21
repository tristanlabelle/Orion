using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.GameLogic.Pathfinding
{
    partial class Pathfinder
    {
        private sealed class OpenList
        {
            public sealed class Bucket
            {
                #region Fields
                public int[] Buffer;
                public int Count;
                #endregion

                #region Constructors
                public Bucket()
                {
                    this.Buffer = new int[16];
                }
                #endregion

                #region Methods
                public void Add(int value)
                {
                    if (Count == Buffer.Length)
                    {
                        uint newBufferLength = PowerOfTwo.Ceiling((uint)(Count + 1));
                        int[] newBuffer = new int[newBufferLength];
                        for (int i = 0; i < Count; ++i)
                            newBuffer[i] = Buffer[i];
                        Buffer = newBuffer;
                    }

                    Buffer[Count] = value;
                    ++Count;
                }

                public void Remove(int value)
                {
                    for (int i = 0; i < Count; ++i)
                    {
                        if (Buffer[i] == value)
                        {
                            Buffer[i] = Buffer[Count - 1];
                            --Count;
                            return;
                        }
                    }
                }

                public void Clear()
                {
                    Count = 0;
                }
                #endregion
            }

            #region Fields
            private const int bucketCount = 16;
            private const int bucketMask = bucketCount - 1;

            private readonly Pathfinder pathfinder;
            private readonly Bucket[] buckets;
            private int count;
            #endregion

            #region Constructors
            public OpenList(Pathfinder pathfinder)
            {
                this.pathfinder = pathfinder;
                this.buckets = new Bucket[bucketCount];
                for (int bucketIndex = 0; bucketIndex < bucketCount; ++bucketIndex)
                    this.buckets[bucketIndex] = new Bucket();

                Debug.Assert(PowerOfTwo.Is(bucketCount));
                Debug.Assert(bucketMask == bucketCount - 1);
            }
            #endregion

            #region Properties
            public int Count
            {
                get { return count; }
            }

            public int Cheapest
            {
                get
                {
                    int cheapestNodeIndex = -1;
                    PathNode cheapestNode = default(PathNode);
                    for (int bucketIndex = 0; bucketIndex < bucketCount; ++bucketIndex)
                    {
                        Bucket bucket = buckets[bucketIndex];
                        for (int elementIndex = 0; elementIndex < bucket.Count; ++elementIndex)
                        {
                            int nodeIndex = bucket.Buffer[elementIndex];
                            PathNode node = pathfinder.nodes[nodeIndex];
                            if (cheapestNodeIndex == -1 || node.TotalCost < cheapestNode.TotalCost)
                            {
                                cheapestNodeIndex = nodeIndex;
                                cheapestNode = node;
                            }
                        }
                    }

                    return cheapestNodeIndex;
                }
            }
            #endregion

            #region Methods
            public void Add(int value)
            {
                buckets[value & bucketMask].Add(value);
                ++count;
            }

            public void Remove(int value)
            {
                buckets[value & bucketMask].Remove(value);
                --count;
            }

            public void Clear()
            {
                for (int i = 0; i < bucketCount; ++i)
                    buckets[i].Clear();
                count = 0;
            }

            public static int[] BufferAllocator(int minLength)
            {
                int length = minLength;
                if (length < 16) length = 16;
                else length = (int)PowerOfTwo.Ceiling((uint)minLength);
                return new int[length];
            }
            #endregion
        }
    }
}
