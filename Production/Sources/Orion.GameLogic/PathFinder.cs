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
        List<PathNode> openNode = new List<PathNode>();
        HashSet<Point> closedNodes = new HashSet<Point>();
        #endregion

        #region constructor
        /// <summary>
        /// Create a New Object PathFinder that return an object Path.
        /// </summary>
        /// <param name="source">The Position of the unit</param>
        /// <param name="destination">The destination point</param>
        public PathFinder(Vector2 source, Vector2 destination)
        {
            Argument.EnsureNotNull(source, "sourcePoint");
            Argument.EnsureNotNull(destination, "destinationPoint");
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

        private void GetNearPointToAdd(PathNode currentNode)
        {
            for (int x = -1; x <= 1; x++) // colonne 
            {
                for (int y = -1; y <= 1; y++) // ligne
                {
                    if (y == 0 && x == 0) continue;

                    Point nearNode = new Point(currentNode.Position.X - x, currentNode.Position.Y - y);

                    if (nearNode.X >= 0 && nearNode.Y >= 0 && closedNodes.Contains(nearNode))
                        continue;

                    // && Terrain.isWalkable(currentNode.Position.X - i,currentNode.Position.X - j)

                    PathNode firstNode = openNode.FirstOrDefault(node =>
                                                    node.Position.X == nearNode.X &&
                                                    node.Position.Y == nearNode.Y);
                    if (firstNode != null)
                    {
                        float moveCost = CalculateMoveCost(firstNode, currentNode.TotalCost, DistanceBetweenTwoPoint(firstNode.Position, currentNode.Position));
                        if (firstNode.TotalCost > moveCost)
                        { // If its a better choise to pass thru the current node , overwrite the parent and the move cost
                            firstNode.ParentNode = currentNode;
                            firstNode.TotalCost = moveCost;
                        }
                    } 
                    //Not in any list and Walkable
                    else
                    {
                        PathNode aNode = new PathNode(currentNode, nearNode, 0);
                        aNode.TotalCost = CalculateMoveCost(aNode,currentNode.TotalCost,DistanceBetweenTwoPoint(currentNode.Position,aNode.Position));
                        openNode.Add(aNode);

                    }
                }
            }
        }
        public Path FindPath()
        {
           PathNode currentNode = source;
           while (currentNode.Position != destination)
           {
               closedNodes.Add(currentNode.Position);
               openNode.Remove(currentNode);
               GetNearPointToAdd(currentNode);

               if(openNode.Count > 0)
               {
                   currentNode = openNode[0];
                   foreach (PathNode aNode in openNode)
                   {
                       if (aNode.TotalCost < currentNode.TotalCost)
                           currentNode = aNode;
                   }
                }
               else
               {
                   break;
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
