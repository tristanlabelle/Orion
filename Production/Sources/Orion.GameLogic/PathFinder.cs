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
        List<PathNode> closeNode = new List<PathNode>();
        #endregion

        #region constructor
        /// <summary>
        /// Create a New Object PathFinder that return an object Path.
        /// </summary>
        /// <param name="source">The Position of the unit</param>
        /// <param name="destination">The destination point</param>
        public PathFinder(Point source, Point destination)
        {
            Argument.EnsureNotNull(source, "sourcePoint");
            Argument.EnsureNotNull(destination, "destinationPoint");
            this.source = new PathNode(null,source,0);
            this.destination = destination;
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
            double squareDistance = Math.Pow((a.X - b.X),2.0) + Math.Pow((a.Y - b.Y),2.0);
            return (float)Math.Sqrt(squareDistance);
        }

        private void getNearPointToAdd(PathNode currentNode)
        {
            for (int j = -1; j <= 1; j++) // colonne 
            {
                for (int i = -1; i <= 1; i++) // ligne
                {
                    Point nearNode = new Point(currentNode.Position.X - j, currentNode.Position.Y - i);
                    if (!(i == 0 && j == 0)) // Avoid that addind 0,0 (current node to the open list)
                    {
                        //If it's in the close list
                        if (nearNode.X >= 0 && nearNode.Y >= 0 && 
                            !closeNode.Any(node =>
                            node.Position.X == nearNode.X &&
                            node.Position.Y == nearNode.Y)
                            // && Terrain.isWalkable(currentNode.Position.X - i,currentNode.Position.X - j)
                            )
                        {
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
            }
        }
        public Path FindPath()
        {
           PathNode currentNode = source;
           while (currentNode.Position != destination)
           {
               closeNode.Add(currentNode);
               getNearPointToAdd(currentNode);
               if(openNode.Count >0)
               {
                   //Get Minimum
                //currentNode = openNode.M
                }
               else
               {
                   break;
               }

           }
            

            return null;
        }

        #endregion
    }
}
