using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using System.Drawing;

namespace Orion.GameLogic
{
    class PathFinder
    {
        #region Fields
        Path theFinalPath;
        PathNode source;
        Point destination;
        Dictionary<Point, PathNode> openNodes = new Dictionary<Point, PathNode>();
        HashSet<Point> closedNodes = new HashSet<Point>();
        #endregion

        #region Constructors
        /// <summary>
        /// Create a New Object PathFinder that return an object Path.
        /// </summary>
        /// <param name="source">The Position of the unit</param>
        /// <param name="destination">The destination point</param>
        public PathFinder(Vector2 source, Vector2 destination)
        {
            this.source = new PathNode(null, new Point((int)source.X, (int)source.Y), 0);
            this.destination = new Point((int)destination.X,(int)destination.Y);
            theFinalPath = new Path();
        }
        
        #endregion

        #region Methods
        private float CalculateMoveCost(PathNode aNode, float deplacementCostOfTheParent, float currentDeplacementCost)
        {
            return Math.Abs(aNode.Position.X - destination.X) + Math.Abs(aNode.Position.Y - destination.Y) 
                + deplacementCostOfTheParent + currentDeplacementCost;
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
            bool isWithinBounds = (nearNodeCoords.X >= 0 && nearNodeCoords.Y >= 0);
            if (!isWithinBounds || closedNodes.Contains(nearNodeCoords))
                return;

            // && Terrain.isWalkable(currentNode.Position.X - i,currentNode.Position.X - j)

            PathNode firstNode;
            if (openNodes.TryGetValue(nearNodeCoords, out firstNode))
            {
                float moveCost = CalculateMoveCost(firstNode, currentNode.TotalCost, DistanceBetweenTwoPoint(firstNode.Position, currentNode.Position));
                if (firstNode.TotalCost > moveCost)
                { // If its a better choise to pass thru the current node , overwrite the parent and the move cost
                    firstNode.ParentNode = currentNode;
                    firstNode.TotalCost = moveCost;
                }
            }
            else
            {
                // Add the node to the open list
                PathNode aNode = new PathNode(currentNode, nearNodeCoords, 0);
                aNode.TotalCost = CalculateMoveCost(aNode, currentNode.TotalCost, DistanceBetweenTwoPoint(currentNode.Position, aNode.Position));
                openNodes.Add(nearNodeCoords, aNode);
            }
        }

        public Path FindPath()
        {
           PathNode currentNode = source;
           while (currentNode.Position != destination)
           {
               closedNodes.Add(currentNode.Position);
               openNodes.Remove(currentNode.Position);
               AddNearbyNodes(currentNode);

               if (openNodes.Count == 0) break;
               
               currentNode = openNodes.First().Value;
               foreach (PathNode aNode in openNodes.Values)
               {
                   if (aNode.TotalCost < currentNode.TotalCost)
                       currentNode = aNode;
               }
           }

           if (currentNode.Position == destination)
           {
               Path path = new Path();
               while (currentNode != source)
               {
                   path.AddNode(currentNode);
                   currentNode = currentNode.ParentNode;
               }
               return path;
           }
           return null;
        }

        #endregion
    }
}
