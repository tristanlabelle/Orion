using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.ObjectModel;

using OpenTK.Math;

namespace Orion.GameLogic
{
    public class Path
    {
        #region Fields
        private readonly PathFinder pathFinder;
        private readonly Vector2 source;
        private readonly Point sourcePoint;
        private readonly Vector2 destination;
        private readonly Point destinationPoint;
        private readonly Dictionary<Point, PathNode> openNodes = new Dictionary<Point, PathNode>();
        private readonly HashSet<Point> closedNodes = new HashSet<Point>();
        private readonly ReadOnlyCollection<Point> points;
        #endregion

        #region Constructor
        internal Path(PathFinder pathFinder, Vector2 source, Vector2 destination)
        {
            this.pathFinder = pathFinder;
            this.source = source;
            this.destination = destination;

            sourcePoint = new Point((int)source.X, (int)source.Y);
            destinationPoint = new Point((int)destination.X, (int)destination.Y);
            PathNode sourceNode = new PathNode(null, sourcePoint, 0);

            PathNode currentNode = sourceNode;
            while (currentNode.Position.X != destinationPoint.X || currentNode.Position.Y != destinationPoint.Y)
            {
                closedNodes.Add(currentNode.Position);
                openNodes.Remove(currentNode.Position);
                AddNearbyNodes(currentNode);

                if (openNodes.Count == 0) break;

                currentNode = openNodes.First().Value;
                foreach (PathNode aNode in openNodes.Values)
                {
                    if (CalculateTotalCost(aNode.Position, aNode.MoveCost) <
                        CalculateTotalCost(currentNode.Position, currentNode.MoveCost))
                        currentNode = aNode;
                }
            }

            if (currentNode.Position == destinationPoint)
            {
                List<Point> pointList = new List<Point>();
                while (currentNode != null)
                {
                    pointList.Add(currentNode.Position);
                    currentNode = currentNode.ParentNode;
                }
                pointList.Reverse();
                points = new ReadOnlyCollection<Point>(pointList);
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
        public ReadOnlyCollection<Point> Points
        {
            get { return points; }
        }
        #endregion

        #region Methods
        private float CalculateTotalCost(Point aPoint, float moveCost)
        {
            return Math.Abs(aPoint.X - destinationPoint.X) + Math.Abs(aPoint.Y - destinationPoint.Y)
                + moveCost;
        }

        private float DistanceBetweenTwoPoint(Point a, Point b)
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

        private void AddNearbyNode(PathNode currentNode, int offsetX, int offsetY)
        {
            Point nearNode = new Point(currentNode.Position.X + offsetX, currentNode.Position.Y + offsetY);
            AddNearbyNode(currentNode, nearNode);
        }

        private void AddNearbyNode(PathNode currentNode, Point nearNodeCoords)
        {
            bool isWithinBounds = (nearNodeCoords.X >= 0 && nearNodeCoords.Y >= 0 && 
                nearNodeCoords.X < this.pathFinder.World.Terrain.Width &&
                nearNodeCoords.Y < this.pathFinder.World.Terrain.Height);
            if (!isWithinBounds || 
                closedNodes.Contains(nearNodeCoords)|| 
                pathFinder.World.Terrain.IsWalkable(nearNodeCoords.X,nearNodeCoords.Y))
                return;


            PathNode firstNodeFound;
            if (openNodes.TryGetValue(nearNodeCoords, out firstNodeFound))
            {
                float movementCost =
                    DistanceBetweenTwoPoint(nearNodeCoords, currentNode.Position);

                float cost = currentNode.MoveCost + movementCost;

                if (firstNodeFound.MoveCost > cost)
                {
                    // If its a better choise to pass thru the current node , overwrite the parent and the move cost
                    firstNodeFound.ParentNode = currentNode;
                    firstNodeFound.MoveCost = cost;
                }
            }
            else
            {
                // Add the node to the open list
                PathNode aNode = new PathNode(currentNode, nearNodeCoords, 0);
                float movementCost = DistanceBetweenTwoPoint(currentNode.Position, aNode.Position);
                aNode.MoveCost = currentNode.MoveCost + movementCost;
                openNodes.Add(nearNodeCoords, aNode);
            }
        }
        #endregion
    }
}
