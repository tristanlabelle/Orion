using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using OpenTK.Math;

namespace Orion.GameLogic.Pathfinding
{
    public class Path
    {
        #region Fields
        private readonly Pathfinder pathFinder;
        private readonly Vector2 source;
        private readonly Point16 sourcePoint;
        private readonly Vector2 destination;
        private readonly Point16 destinationPoint;
        private readonly Dictionary<Point16, PathNode> openNodes = new Dictionary<Point16, PathNode>();
        private readonly HashSet<Point16> closedNodes = new HashSet<Point16>();
        private readonly ReadOnlyCollection<Point16> points;
        #endregion

        #region Constructor
        internal Path(Pathfinder pathFinder, Vector2 source, Vector2 destination)
        {
            this.pathFinder = pathFinder;
            this.source = source;
            this.destination = destination;

            sourcePoint = new Point16((short)source.X, (short)source.Y);
            destinationPoint = new Point16((short)destination.X, (short)destination.Y);

            PathNode sourceNode = new PathNode(null, sourcePoint, 0, GetCostToDestination(sourcePoint));

            PathNode currentNode = sourceNode;
            while (currentNode.Position.X != destinationPoint.X || currentNode.Position.Y != destinationPoint.Y)
            {
                closedNodes.Add(currentNode.Position);
                openNodes.Remove(currentNode.Position);
                AddNearbyNodes(currentNode);

                if (openNodes.Count == 0) break;

                currentNode = openNodes.First().Value;
                foreach (PathNode openNode in openNodes.Values)
                {
                    if (openNode.TotalCost < currentNode.TotalCost)
                        currentNode = openNode;
                }
            }

            if (currentNode.Position == destinationPoint)
            {
                List<Point16> pointList = new List<Point16>();
                while (currentNode != null)
                {
                    pointList.Add(currentNode.Position);
                    currentNode = currentNode.ParentNode;
                }
                pointList.Reverse();
                points = new ReadOnlyCollection<Point16>(pointList);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this path was successfully found.
        /// </summary>
        internal bool Succeeded
        {
            get { return points != null; }
        }

        /// <summary>
        /// Gets the point that is at the source of this path.
        /// </summary>
        public Vector2 Source
        {
            get { return source; }
        }

        /// <summary>
        /// Gets the destination point of this path.
        /// </summary>
        public Vector2 Destination
        {
            get { return destination; }
        }

        /// <summary>
        /// Gets the sequence of points that trace this path.
        /// </summary>
        public ReadOnlyCollection<Point16> Points
        {
            get { return points; }
        }
        #endregion

        #region Methods
        private float GetTotalCost(Point16 point, float movementCost)
        {
            return GetCostToDestination(point) + movementCost;
        }

        private float GetCostToDestination(Point16 point)
        {
            return Math.Abs(point.X - destinationPoint.X) + Math.Abs(point.Y - destinationPoint.Y);
        }

        private float DistanceBetweenTwoPoint(Point16 a, Point16 b)
        {
            int deltaX = a.X - b.X;
            int deltaY = a.Y - b.Y;
            double squaredDistance = deltaX * deltaX + deltaY * deltaY;
            return (float)Math.Sqrt(squaredDistance);
        }

        private void AddNearbyNodes(PathNode currentNode)
        {
            // # # #
            // #   #
            // # # #
            AddNearbyNode(currentNode, -1, -1);
            AddNearbyNode(currentNode, 0, -1);
            AddNearbyNode(currentNode, 1, -1);
            AddNearbyNode(currentNode, -1, 0);
            AddNearbyNode(currentNode, 1, 0);
            AddNearbyNode(currentNode, -1, 1);
            AddNearbyNode(currentNode, 0, 1);
            AddNearbyNode(currentNode, 1, 1);
        }

        private void AddNearbyNode(PathNode currentNode, short offsetX, short offsetY)
        {
            int x = currentNode.Position.X + offsetX;
            int y = currentNode.Position.Y + offsetY;
            Point16 nearNode = new Point16((short)x, (short)y);
            AddNearbyNode(currentNode, nearNode);
        }

        private void AddNearbyNode(PathNode currentNode, Point16 nearNodeCoords)
        {
            bool isWithinBounds = (nearNodeCoords.X >= 0 && nearNodeCoords.Y >= 0 && 
                nearNodeCoords.X < this.pathFinder.World.Terrain.Width &&
                nearNodeCoords.Y < this.pathFinder.World.Terrain.Height);

            if (!isWithinBounds || closedNodes.Contains(nearNodeCoords)
                || !pathFinder.World.Terrain.IsWalkable(nearNodeCoords.X,nearNodeCoords.Y))
                return;

            float movementCost = DistanceBetweenTwoPoint(currentNode.Position, nearNodeCoords);
            float costFromSource = currentNode.CostFromSource + movementCost;

            PathNode firstNodeFound;
            if (openNodes.TryGetValue(nearNodeCoords, out firstNodeFound))
            {
                if (costFromSource < firstNodeFound.CostFromSource)
                {
                    // If its a better choise to pass through the current node, overwrite the parent and the move cost
                    firstNodeFound.ParentNode = currentNode;
                    float costToDestination = GetCostToDestination(nearNodeCoords);
                    firstNodeFound.SetCost(costFromSource, costToDestination);
                }
            }
            else
            {
                // Add the node to the open list
                float costToDestination = GetCostToDestination(nearNodeCoords);
                PathNode aNode = new PathNode(currentNode, nearNodeCoords, costFromSource, costToDestination);
                openNodes.Add(nearNodeCoords, aNode);
            }
        }
        #endregion
    }
}
