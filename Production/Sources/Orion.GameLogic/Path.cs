using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Orion.GameLogic
{
    public class Path
    {
        #region Fields
        LinkedList<PathNode> linkList;
        PathFinder pathFinder;
        Point destination;
        Dictionary<Point, PathNode> openNodes;
        HashSet<Point> closedNodes;

        #endregion

        #region Constructor
        public Path(Point source, Point destinationPoint, PathFinder pathFinder)
        {
            linkList = new LinkedList<PathNode>();
            openNodes = new Dictionary<Point, PathNode>();
            closedNodes = new HashSet<Point>();

            this.pathFinder = pathFinder;

            PathNode sourceNode = new PathNode(null, new Point((int)source.X, (int)source.Y), 0);
            this.destination = destinationPoint;

            PathNode currentNode = sourceNode;
            while (currentNode.Position.X != destination.X || currentNode.Position.Y != destination.Y)
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
                while (currentNode != null)
                {
                    AddNode(currentNode);
                    currentNode = currentNode.ParentNode;
                }
            }
            linkList =  null;
        }
        #endregion
      
        
        #region Methods
        public void AddNode(PathNode node)
        {
            linkList.AddFirst(node);
        }


        private float CalculateTotalCost(Point aPoint, float moveCost)
        {
            return Math.Abs(aPoint.X - destination.X) + Math.Abs(aPoint.Y - destination.Y)
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
                { // If its a better choise to pass thru the current node , overwrite the parent and the move cost
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

        #region Proprieties

        public List<PathNode> List
        {
            get { return linkList.ToList(); }
        }

        #endregion
    }
}
