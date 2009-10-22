using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    internal sealed class PathNode
    {
        #region Fields
        private PathNode parentNode;
        private Point16 position;
        private float costFromSource;
        private float totalCost; // Stored instead of costToDestination as it is most often accessed.
        #endregion

        #region Properties
        public PathNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        public Point16 Position
        {
            get { return position; }
        }

        public float CostFromSource
        {
            get { return costFromSource; }
        }

        public float EstimatedCostToDestination
        {
            get { return totalCost - costFromSource; }
        }

        public float TotalCost
        {
            get { return totalCost; }
        }
        #endregion

        #region Methods
        public void Reset(PathNode parentNode, Point16 position,
            float costFromSource, float estimatedCostToDestination)
        {
            this.parentNode = parentNode;
            this.position = position;
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }

        public void SetCosts(float costFromSource, float estimatedCostToDestination)
        {
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }
        #endregion
    }
}
