using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;

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
        private readonly List<Point> points = new List<Point>();
        private Func<Point, float> destinationDistanceEvaluator;
        private Func<Point, bool> isWalkable;
        private PathNode nodeNearestToDestination;
        private int maxNodesToVisit;
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
        #endregion

        #region Methods
        /// <summary>
        /// Finds a path to go from a source position to a destination position
        /// by taking into account tile solidity.
        /// </summary>
        /// <param name="source">The position where the path starts.</param>
        /// <param name="distanceEvaluator">A delegate to a method which evaluates the distance to the destination.</param>
        /// <param name="isWalkable">A delegate to a method which evaluates if a given tile is walkable.</param>
        /// <param name="maxNodesToVisit">The maximum number of nodes to visit before giving up.</param>
        /// <returns>The path that was found, or <c>null</c> is none was.</returns>
        public Path Find(Point source, Func<Point, float> destinationDistanceEvaluator,
            Func<Point, bool> isWalkable, int maxNodesToVisit)
        {
            Argument.EnsureNotNull(destinationDistanceEvaluator, "distanceEvaluator");
            Argument.EnsureNotNull(isWalkable, "isWalkable");

            CleanUp();

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
            this.isWalkable = isWalkable;
            Point16 sourcePoint = RoundCoordinates(source);
            this.maxNodesToVisit = maxNodesToVisit;

            PathNode destinationNode = FindPathNodes(sourcePoint);

            bool complete = true;
            if (destinationNode == null)
            {
                Debug.Assert(nodeNearestToDestination != null);
                destinationNode = nodeNearestToDestination;
                complete = false;
            }

            FindPathPointsTo(destinationNode);
            CheaplySmoothPathPoints();
            return new Path(points, complete);
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
                    if (!Bresenham.GetPoints(lineSegment, 3).All(isWalkable))
                        break;
                    points.RemoveAt(i + 1);
                }
            }
        }

        private void CheaplySmoothPathPoints()
        {
            for (int i = 0; i < points.Count - 2; ++i)
            {
                Point sourcePoint = points[i];
                while (i != points.Count - 2)
                {
                    Point destinationPoint = points[i + 2];

                    Region region = Region.FromPoints(sourcePoint, destinationPoint);
                    if (region.Area >= 40 || !region.Points.All(isWalkable))
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

            nodeNearestToDestination = null;
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

        private PathNode GetPathNode(PathNode parentNode, Point position,
            float costFromSource, float distanceToDestination)
        {
            PathNode node = nodePool.Get();
            node.Reset(parentNode, (Point16)position, costFromSource, distanceToDestination);

            if (nodeNearestToDestination == null
                || distanceToDestination < nodeNearestToDestination.DistanceToDestination)
            {
                nodeNearestToDestination = node;
            }

            return node;
        }

        /// <summary>
        /// Finds a path to the destination point, creating the needed nodes along the way.
        /// </summary>
        /// <returns>The destination node, if a path is found getting to it.</returns>
        private PathNode FindPathNodes(Point16 sourcePoint)
        {
            float distanceToDestination = destinationDistanceEvaluator(sourcePoint);
            PathNode sourceNode = GetPathNode(null, sourcePoint, 0, distanceToDestination);

            PathNode currentNode = sourceNode;
            while (currentNode.DistanceToDestination > 0.0001f)
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
            return openNodes.Values.WithMinOrDefault(node => node.TotalCost);
        }

        private float GetDistance(Point a, Point b)
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
            if (!IsOpenable(new Point(currentNode.Point.X + offsetX, currentNode.Point.Y))
                || !IsOpenable(new Point(currentNode.Point.X, currentNode.Point.Y + offsetY)))
                return;

            AddAdjacentNode(currentNode, offsetX, offsetY);
        }

        private void AddAdjacentNode(PathNode currentNode, int offsetX, int offsetY)
        {
            int x = currentNode.Point.X + offsetX;
            int y = currentNode.Point.Y + offsetY;
            Point nearNode = new Point(x, y);
            AddNearbyNode(currentNode, nearNode);
        }

        private bool IsOpenable(Point nearbyPoint)
        {
            return !closedNodes.ContainsKey((Point16)nearbyPoint)
                && isWalkable(nearbyPoint);
        }

        private void AddNearbyNode(PathNode currentNode, Point nearbyPoint)
        {
            if (!IsOpenable(nearbyPoint)) return;

            float movementCost = GetDistance(currentNode.Point, nearbyPoint);
            float costFromSource = currentNode.CostFromSource + movementCost;

            PathNode nearbyNode;
            if (openNodes.TryGetValue((Point16)nearbyPoint, out nearbyNode))
            {
                if (costFromSource < nearbyNode.CostFromSource)
                {
                    // If it is a better choice to pass through the current node, overwrite the parent and the move cost
                    float estimatedCostToDestination = destinationDistanceEvaluator(nearbyPoint);
                    nearbyNode.ChangeSource(currentNode, costFromSource, estimatedCostToDestination);
                }
            }
            else
            {
                // Add the node to the open list
                float estimatedCostToDestination = destinationDistanceEvaluator(nearbyPoint);
                nearbyNode = GetPathNode(currentNode, nearbyPoint, costFromSource, estimatedCostToDestination);
                openNodes.Add((Point16)nearbyPoint, nearbyNode);
            }
        }

        private bool IsSameDirection(Point a, Point b, Point c)
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
