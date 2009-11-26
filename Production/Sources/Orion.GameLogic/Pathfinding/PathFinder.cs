using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Pathfinding
{
    /// <summary>
    /// Finds paths to go from one point to another in a grid-based environment.
    /// </summary>
    public sealed class Pathfinder
    {
        #region Fields
        private readonly Pool<PathNode> nodePool = new Pool<PathNode>();
        private readonly Dictionary<Point16, PathNode> openNodes = new Dictionary<Point16, PathNode>();
        private readonly Dictionary<Point16, PathNode> closedNodes = new Dictionary<Point16, PathNode>();
        private readonly List<Vector2> points = new List<Vector2>();
        private Func<Point, bool> isWalkable;
        private Point16 destinationPoint;
        private int maxNodesToVisit = int.MaxValue;
        #endregion

        #region Properties
        public IEnumerable<PathNode> OpenNodes
        {
            get { return openNodes.Values; }
        }

        public IEnumerable<PathNode> ClosedNodes
        {
            get { return closedNodes.Values; }
        }

        public int MaxNodesToVisit
        {
            get { return maxNodesToVisit; }
            set
            {
                Argument.EnsureNotNull(maxNodesToVisit, "maxNodesToVisit");
                maxNodesToVisit = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds a path to go from a source position to a destination position
        /// by taking into account tile solidity.
        /// </summary>
        /// <param name="source">The position where the path starts.</param>
        /// <param name="destination">The position the path should reach.</param>
        /// <param name="isWalkable">A delegate to a method which evaluates if a given tile is walkable.</param>
        /// <returns>The path that was found, or <c>null</c> is none was.</returns>
        public Path Find(Vector2 source, Vector2 destination, Func<Point, bool> isWalkable)
        {
            Argument.EnsureNotNull(isWalkable, "isWalkable");

            CleanUp();

            this.isWalkable = isWalkable;
            Point16 sourcePoint = RoundCoordinates(source);
            this.destinationPoint = RoundCoordinates(destination);

            PathNode destinationNode = FindPathNodes(sourcePoint);

            if (destinationNode == null) destinationNode = FindClosedNodeNearestToDestination();

            FindPathPointsTo(destinationNode);
            SmoothPathPoints();
            return new Path(source, destination, points);
        }

        private void SmoothPathPoints()
        {
            for (int i = 0; i < points.Count - 2; ++i)
            {
                Vector2 sourcePoint = points[i];
                while (i != points.Count - 2)
                {
                    Vector2 destinationPoint = points[i + 2];

                    // Extend the line segment by 1 in both directions to be sure
                    // the shortcut is really walkable.
                    Vector2 normalizedDelta = Vector2.NormalizeFast(destinationPoint - sourcePoint);
                    LineSegment lineSegment = new LineSegment(sourcePoint - normalizedDelta, destinationPoint + normalizedDelta);
                    if (!Bresenham.All(lineSegment, 3, isWalkable))
                        break;
                    points.RemoveAt(i + 1);
                }
            }
        }

        private void CleanUp()
        {
            // Return the path nodes to the internal pool of nodes.
            foreach (PathNode node in openNodes.Values)
                nodePool.Add(node);
            openNodes.Clear();

            foreach (PathNode node in closedNodes.Values)
                nodePool.Add(node);
            closedNodes.Clear();

            points.Clear();
        }

        private PathNode FindClosedNodeNearestToDestination()
        {
            return closedNodes.Values.WithMinOrDefault(node => node.EstimatedCostToDestination);
        }

        private void FindPathPointsTo(PathNode destinationNode)
        {
            PathNode currentNode = destinationNode;
            while (currentNode != null)
            {
                points.Add(currentNode.Point);
                currentNode = currentNode.Source;
            }

            points.Reverse();
        }

        private PathNode GetPathNode(PathNode parentNode, Point16 position,
            float costFromSource, float estimatedCostToDestination)
        {
            PathNode node = nodePool.Get();
            node.Reset(parentNode, position, costFromSource, estimatedCostToDestination);
            return node;
        }

        /// <summary>
        /// Finds a path to the destination point, creating the needed nodes along the way.
        /// </summary>
        /// <returns>The destination node, if a path is found getting to it.</returns>
        private PathNode FindPathNodes(Point16 sourcePoint)
        {
            float estimatedCostFromSourceToDestination = GetMovementCost(sourcePoint, destinationPoint);
            PathNode sourceNode = GetPathNode(null, sourcePoint, 0, estimatedCostFromSourceToDestination);

            PathNode currentNode = sourceNode;
            while (currentNode.Point.X != destinationPoint.X || currentNode.Point.Y != destinationPoint.Y)
            {
                closedNodes.Add(currentNode.Point, currentNode);
                openNodes.Remove(currentNode.Point);
                AddNearbyNodes(currentNode);

                if (openNodes.Count == 0 || openNodes.Count + closedNodes.Count > maxNodesToVisit)
                    return null;

                currentNode = GetCheapestOpenNode();
            }

            return currentNode;
        }

        private PathNode GetCheapestOpenNode()
        {
            return openNodes.Values.WithMinOrDefault(node => node.EstimatedCostToDestination);
        }

        private float GetMovementCost(Point16 a, Point16 b)
        {
            return ((Vector2)a - (Vector2)b).LengthFast;
        }

        private void AddNearbyNodes(PathNode currentNode)
        {
            AddDiagonalAdjacentNode(currentNode, -1, -1);
            AddAdjacentNode(currentNode, 0, -1);
            AddDiagonalAdjacentNode(currentNode, 1, -1);
            AddAdjacentNode(currentNode, -1, 0);
            AddAdjacentNode(currentNode, 1, 0);
            AddDiagonalAdjacentNode(currentNode, -1, 1);
            AddAdjacentNode(currentNode, 0, 1);
            AddDiagonalAdjacentNode(currentNode, 1, 1);
        }

        private void AddDiagonalAdjacentNode(PathNode currentNode, int offsetX, int offsetY)
        {
            // Disallow going from A to B in situations like (# is non-walkable):
            ///
            // #B
            // A#
            if (!IsOpenable(new Point16((short)(currentNode.Point.X + offsetX), currentNode.Point.Y))
                || !IsOpenable(new Point16(currentNode.Point.X, (short)(currentNode.Point.Y + offsetY))))
                return;

            AddAdjacentNode(currentNode, offsetX, offsetY);
        }

        private void AddAdjacentNode(PathNode currentNode, int offsetX, int offsetY)
        {
            int x = currentNode.Point.X + offsetX;
            int y = currentNode.Point.Y + offsetY;
            Point16 nearNode = new Point16((short)x, (short)y);
            AddNearbyNode(currentNode, nearNode);
        }

        private bool IsOpenable(Point16 nearbyPoint)
        {
            return !closedNodes.ContainsKey(nearbyPoint)
                && isWalkable(nearbyPoint);
        }

        private void AddNearbyNode(PathNode currentNode, Point16 nearbyPoint)
        {
            if (!IsOpenable(nearbyPoint)) return;

            float movementCost = GetMovementCost(currentNode.Point, nearbyPoint);
            if (currentNode.Source != null)
            {
                // Discourage turns
                bool isTurning = !IsSameDirection(currentNode.Source.Point, currentNode.Point, nearbyPoint);
                if (isTurning) ++movementCost;
            }

            float costFromSource = currentNode.CostFromSource + movementCost;

            PathNode nearbyNode;
            if (openNodes.TryGetValue(nearbyPoint, out nearbyNode))
            {
                if (costFromSource < nearbyNode.CostFromSource)
                {
                    // If it is a better choice to pass through the current node, overwrite the parent and the move cost
                    float estimatedCostToDestination = GetMovementCost(nearbyPoint, destinationPoint);
                    nearbyNode.ChangeSource(currentNode, costFromSource, estimatedCostToDestination);
                }
            }
            else
            {
                // Add the node to the open list
                float estimatedCostToDestination = GetMovementCost(nearbyPoint, destinationPoint);
                nearbyNode = GetPathNode(currentNode, nearbyPoint, costFromSource, estimatedCostToDestination);
                openNodes.Add(nearbyPoint, nearbyNode);
            }
        }

        private bool IsSameDirection(Point16 a, Point16 b, Point16 c)
        {
            return (c.X - b.X) == (b.X - a.X) && (c.Y - b.Y) == (b.Y - a.Y);
        }

        private Point16 RoundCoordinates(Vector2 point)
        {
            return new Point16((short)point.X, (short)point.Y);
        }
        #endregion
    }
}
