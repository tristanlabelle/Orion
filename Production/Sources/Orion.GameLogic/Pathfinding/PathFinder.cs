using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;
using System.Collections;

namespace Orion.GameLogic.Pathfinding
{
    /// <summary>
    /// Finds paths to go from one point to another in a grid-based environment.
    /// </summary>
    public sealed class Pathfinder
    {
        #region Fields
        private readonly Size gridSize;
        private readonly PathNode[] nodes;
        private readonly BitArray closedNodes;
        private readonly HashSet<int> openNodeIndices = new HashSet<int>();
        private readonly List<Point> points = new List<Point>();
        private Func<Point, float> destinationDistanceEvaluator;
        private Func<Point, bool> isWalkable;
        private int startPointIndex;
        private int nodeNearestToDestinationIndex;
        private int maxNodesToVisit;
        private int visitedNodeCount;
        #endregion

        #region Constructors
        public Pathfinder(Size gridSize)
        {
            this.gridSize = gridSize;
            this.nodes = new PathNode[gridSize.Area];
            this.closedNodes = new BitArray(gridSize.Area);
        }
        #endregion

        #region Methods
        #region Public
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
            this.startPointIndex = PointToIndex(source);
            this.nodeNearestToDestinationIndex = startPointIndex;
            this.maxNodesToVisit = maxNodesToVisit;

            int endPointIndex = FindPathNodes();
            Point endPoint = IndexToPoint(endPointIndex);
            PathNode endNode = nodes[endPointIndex];

            FindPathPointsTo(endPoint);
            SmoothPathPoints();
            return new Path(points, IsDestination(endNode));
        }
        #endregion

        #region Smoothing
        private void SmoothPathPoints()
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
        #endregion

        private void CleanUp()
        {
            closedNodes.SetAll(false);
            openNodeIndices.Clear();

            points.Clear();

            visitedNodeCount = 0;
            nodeNearestToDestinationIndex = -1;
        }

        private void FindPathPointsTo(Point endPoint)
        {
            int currentPointIndex = PointToIndex(endPoint);
            while (true)
            {
                Point currentPoint = IndexToPoint(currentPointIndex);
                points.Add(currentPoint);

                PathNode currentNode = nodes[currentPointIndex];

                int parentNodeIndex = currentNode.ParentNodeIndex;
                if (parentNodeIndex == -1) break;

                currentPointIndex = parentNodeIndex;
            }

            points.Reverse();
        }

        #region Node queries
        private int PointToIndex(Point point)
        {
            return point.Y * gridSize.Width + point.X;
        }

        private Point IndexToPoint(int index)
        {
            return new Point(index % gridSize.Width, index / gridSize.Width);
        }

        private bool IsDestination(PathNode node)
        {
            return node.DistanceToDestination < 0.001f;
        }

        private void Open(Point nodePoint, int parentNodeIndex, float costFromSource)
        {
            int nodeIndex = PointToIndex(nodePoint);

            float distanceToDestination = destinationDistanceEvaluator(nodePoint);

            PathNode node = new PathNode(parentNodeIndex, costFromSource, distanceToDestination);
            nodes[nodeIndex] = node;
            openNodeIndices.Add(nodeIndex);

            if (nodeNearestToDestinationIndex == -1
                || distanceToDestination < nodes[nodeNearestToDestinationIndex].DistanceToDestination)
            {
                nodeNearestToDestinationIndex = nodeIndex;
            }

            ++visitedNodeCount;
        }

        private void Close(int nodeIndex)
        {
            Debug.Assert(!closedNodes[nodeIndex]);
            openNodeIndices.Remove(nodeIndex);
            closedNodes[nodeIndex] = true;
        }

        private bool IsOpenable(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= gridSize.Width || point.Y >= gridSize.Height)
                return false;

            int nodeIndex = PointToIndex(point);
            return !closedNodes[nodeIndex] && isWalkable(point);
        }
        #endregion

        private int FindPathNodes()
        {
            Point startPoint = IndexToPoint(startPointIndex);
            Open(startPoint, -1, 0);
            while (true)
            {
                int currentNodeIndex = GetCheapestOpenNodeIndex();
                PathNode currentNode = nodes[currentNodeIndex];
                if (IsDestination(currentNode)) return currentNodeIndex;

                Close(currentNodeIndex);

                Point currentNodePoint = IndexToPoint(currentNodeIndex);
                AddNearbyNodes(currentNodePoint);

                if (openNodeIndices.Count == 0 || visitedNodeCount >= maxNodesToVisit)
                    return nodeNearestToDestinationIndex;
            }
        }

        private int GetCheapestOpenNodeIndex()
        {
            using (var enumerator = openNodeIndices.GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new Exception("Expected at least one open node.");

                int cheapestNodeIndex = enumerator.Current;
                PathNode cheapestNode = nodes[cheapestNodeIndex];
                while (enumerator.MoveNext())
                {
                    int nodeIndex = enumerator.Current;
                    PathNode node = nodes[nodeIndex];
                    if (node.TotalCost < cheapestNode.TotalCost)
                    {
                        cheapestNodeIndex = nodeIndex;
                        cheapestNode = node;
                    }
                }

                return cheapestNodeIndex;
            }
        }

        private float GetDistance(Point a, Point b)
        {
            return ((Vector2)a - (Vector2)b).LengthFast;
        }

        #region Adding Nodes
        private void AddNearbyNodes(Point currentNodePoint)
        {
            AddDiagonalAdjacentNode(currentNodePoint, -1, -1);
            AddAdjacentNode(currentNodePoint, 0, -1);
            AddDiagonalAdjacentNode(currentNodePoint, 1, -1);
            AddAdjacentNode(currentNodePoint, -1, 0);
            AddAdjacentNode(currentNodePoint, 1, 0);
            AddDiagonalAdjacentNode(currentNodePoint, -1, 1);
            AddAdjacentNode(currentNodePoint, 0, 1);
            AddDiagonalAdjacentNode(currentNodePoint, 1, 1);
        }

        private void AddDiagonalAdjacentNode(Point currentNodePoint, int offsetX, int offsetY)
        {
            // Disallow going from A to B in situations like (# is non-walkable):
            ///
            // #B
            // A#
            if (!IsOpenable(new Point(currentNodePoint.X + offsetX, currentNodePoint.Y))
                || !IsOpenable(new Point(currentNodePoint.X, currentNodePoint.Y + offsetY)))
                return;

            AddAdjacentNode(currentNodePoint, offsetX, offsetY);
        }

        private void AddAdjacentNode(Point currentNodePoint, int offsetX, int offsetY)
        {
            int x = currentNodePoint.X + offsetX;
            int y = currentNodePoint.Y + offsetY;
            Point adjacentPoint = new Point(x, y);
            AddAdjacentNode(currentNodePoint, adjacentPoint);
        }

        private void AddAdjacentNode(Point currentNodePoint, Point adjacentPoint)
        {
            if (!IsOpenable(adjacentPoint)) return;

            int currentNodeIndex = PointToIndex(currentNodePoint);
            PathNode currentNode = nodes[currentNodeIndex];

            float movementCost = GetDistance(currentNodePoint, adjacentPoint);
            float costFromSource = currentNode.CostFromSource + movementCost;

            int adjacentNodeIndex = PointToIndex(adjacentPoint);
            if (openNodeIndices.Contains(adjacentNodeIndex))
            {
                PathNode adjacentNode = nodes[adjacentNodeIndex];
                if (costFromSource < adjacentNode.CostFromSource)
                {
                    // If it is a better choice to pass through the current node, overwrite the source and the move cost
                    float distanceToDestination = destinationDistanceEvaluator(adjacentPoint);
                    adjacentNode = new PathNode(currentNodeIndex, costFromSource, distanceToDestination);
                    nodes[adjacentNodeIndex] = adjacentNode;
                }
            }
            else
            {
                // Add the node to the open list
                Open(adjacentPoint, currentNodeIndex, costFromSource);
            }
        }
        #endregion

        private bool IsSameDirection(Point a, Point b, Point c)
        {
            return (c.X - b.X) == (b.X - a.X) && (c.Y - b.Y) == (b.Y - a.Y);
        }
        #endregion
    }
}
