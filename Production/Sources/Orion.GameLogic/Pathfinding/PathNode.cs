﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class PathNode
    {
        #region Fields
        private PathNode parentNode;
        private readonly Point16 position;
        private float costFromSource;
        private float totalCost; // Stored instead of costToDestination as it is most often accessed.
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for a Node (path of terrain).
        /// </summary>
        /// <param name="parentNode">The parent represent the fastest way to move from a nearby node.</param>
        /// <param name="position">The x y position of the node in the world</param>
        /// <param name="costFromSource">
        /// The cumulative cost of the movement from the beginning of the path to this node.
        /// </param>
        /// <param name="estimatedCostToDestination">
        /// The estimated cost to reach the destination from this node.
        /// </param>
        public PathNode(PathNode parentNode, Point16 position,
            float costFromSource, float estimatedCostToDestination)
        {
            this.parentNode = parentNode;
            this.position = position;
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }
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
        public void SetCosts(float costFromSource, float estimatedCostToDestination)
        {
            this.costFromSource = costFromSource;
            this.totalCost = costFromSource + estimatedCostToDestination;
        }
        #endregion
    }
}
