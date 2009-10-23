using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Pathfinding
{
    internal sealed class PathNodeQueue
    {
        #region Fields
        private PathNode[] nodes = new PathNode[256];
        private int count;
        #endregion

        #region Properties
        public int Capacity
        {
            get { return nodes.Length; }
        }

        public int Count
        {
            get { return count; }
        }

        public PathNode Minimum
        {
            get
            {
                if (count == 0)
                    throw new InvalidOperationException("Cannot retrieve the minimum node of an empty queue.");

                return nodes[0];
            }
        }
        #endregion

        #region Methods
        public void Enqueue(PathNode node)
        {
            Argument.EnsureNotNull(node, "node");

            GrowIfFull();
            nodes[count] = node;
            ++count;
            BubbleTowardsRoot(count - 1);
        }

        public PathNode Dequeue()
        {
            if(count == 0)
                throw new InvalidOperationException("Cannot dequeue an empty queue.");

            PathNode result = nodes[0];
            nodes[0] = nodes[nodes.Length - 1];
            --count;
            BubbleTowardsLeaves(0);
            return result;
        }

        public void Update(PathNode node)
        {
            Argument.EnsureNotNull(node, "node");
            int index = Array.IndexOf(nodes, node);
            if (index == -1) throw new ArgumentException("No such node in the queue.", "node");
            BubbleTowardsRoot(index);
            BubbleTowardsLeaves(index);
        }

        public void Clear()
        {
            count = 0;
        }

        private void GrowIfFull()
        {
            if (count == nodes.Length)
            {
                Array.Resize(ref nodes, nodes.Length * 2);
            }
        }

        private void Swap(int index1, int index2)
        {
            PathNode temp = nodes[index1];
            nodes[index1] = nodes[index2];
            nodes[index2] = temp;
        }

        private void BubbleTowardsRoot(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (nodes[parentIndex].TotalCost <= nodes[index].TotalCost)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void BubbleTowardsLeaves(int index)
        {
            unchecked
            {
                while (true)
                {
                    int firstChildIndex = index * 2 + 1;
                    if (firstChildIndex < Count) break;

                    int lowestChildIndex = firstChildIndex;
                    if (firstChildIndex + 1 < Count
                        && nodes[firstChildIndex].TotalCost > nodes[firstChildIndex + 1].TotalCost)
                    {
                        lowestChildIndex = firstChildIndex + 1;
                    }

                    if (nodes[lowestChildIndex].TotalCost >= nodes[index].TotalCost)
                        break;

                    Swap(lowestChildIndex, index);
                    index = lowestChildIndex;
                }
            }
        }
        #endregion
    }
}
