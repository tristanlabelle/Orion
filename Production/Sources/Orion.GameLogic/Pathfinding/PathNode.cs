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
        public readonly int ParentNodeIndex;
        public readonly float CostFromSource;
        public readonly float TotalCost;
        #endregion

        #region Constructors
        public PathNode(int parentNodeIndex,
            float costFromSource, float distanceToDestination)
        {
            this.ParentNodeIndex = parentNodeIndex;
            this.CostFromSource = costFromSource;
            this.TotalCost = costFromSource + distanceToDestination;
        }
        #endregion

        #region Properties
        public float DistanceToDestination
        {
            get { return TotalCost - CostFromSource; }
        }

        public bool IsDestination
        {
            get { return TotalCost - CostFromSource < 0.001f; }
        }
        #endregion
    }
}
