using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Orion.GameLogic.Pathfinding
{
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = 4 * 3)]
    public struct PathNode
    {
        #region Fields
        private readonly int parentNodeIndex;
        private readonly float costFromSource;
        private readonly float distanceToDestination;
        #endregion

        #region Constructors
        public PathNode(int parentNodeIndex,
            float costFromSource, float distanceToDestination)
        {
            this.parentNodeIndex = parentNodeIndex;
            this.costFromSource = costFromSource;
            this.distanceToDestination = distanceToDestination;
        }
        #endregion

        #region Properties
        public int ParentNodeIndex
        {
            get { return parentNodeIndex; }
        }

        public float CostFromSource
        {
            get { return costFromSource; }
        }

        public float DistanceToDestination
        {
            get { return distanceToDestination; }
        }

        public float TotalCost
        {
            get { return costFromSource + distanceToDestination; }
        }
        #endregion
    }
}
