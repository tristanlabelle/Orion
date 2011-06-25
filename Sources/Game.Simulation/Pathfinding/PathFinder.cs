using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation.Pathfinding
{
    /// <summary>
    /// Finds paths to go from one point to another in a grid-based environment.
    /// </summary>
    public sealed partial class Pathfinder
    {
        #region Fields
        private static readonly float sideMovementCost = GetDistance(new Point(0, 0), new Point(1, 0));
        private static readonly float diagonalMovementCost = GetDistance(new Point(0, 0), new Point(1, 1));

        private readonly Size gridSize;
        private readonly PathNode[] nodes;
        private readonly byte[] nodeStates;
        private readonly OpenList openList;
        private readonly List<Point> points = new List<Point>();
        private Func<Point, float> destinationDistanceEvaluator;
        private Func<Point, bool> isWalkable;
        private int startPointIndex;
        private int nodeNearestToDestinationIndex;
        private int maxNodesToVisit;
        private int visitedNodeCount;
        private byte openNodeStateValue = 1;
        private byte closedNodeStateValue = 2;
        #endregion

        #region Constructors
        public Pathfinder(Size gridSize)
        {
            this.gridSize = gridSize;
            this.nodes = new PathNode[gridSize.Area];
            this.nodeStates = new byte[gridSize.Area];
            this.openList = new OpenList(this);
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
            return new Path(points, endNode.IsDestination);
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
                    if (region.Area >= 40) break;

                    int exclusiveMaxX = region.ExclusiveMaxX;
                    int exclusiveMaxY = region.ExclusiveMaxY;

                    // The following goto is used to break out of three nested loops at once
                    for (int y = region.MinY; y < exclusiveMaxY; ++y)
                        for (int x = region.MinX; x < exclusiveMaxX; ++x)
                            if (!isWalkable(new Point(x, y)))
                                goto cannotSmooth;

                    points.RemoveAt(i + 1);
                }

            cannotSmooth: ;
            }
        }
        #endregion

        private void CleanUp()
        {
            openList.Clear();

            points.Clear();

            visitedNodeCount = 0;
            nodeNearestToDestinationIndex = -1;

            if ((int)openNodeStateValue == (int)byte.MaxValue - 2)
            {
                openNodeStateValue = 1;
                closedNodeStateValue = 2;
                Array.Clear(nodeStates, 0, nodeStates.Length);
            }
            else
            {
                openNodeStateValue = (byte)(openNodeStateValue + 2);
                closedNodeStateValue = (byte)(closedNodeStateValue + 2);
            }
        }

        private void FindPathPointsTo(Point endPoint)
        {
            int currentPointIndex = PointToIndex(endPoint);
            Point beforePreviousPoint = IndexToPoint(currentPointIndex);
            Point previousPoint = beforePreviousPoint;
            Point currentPoint = beforePreviousPoint;
            while (true)
            {
                PathNode currentNode = nodes[currentPointIndex];
                int parentNodeIndex = currentNode.ParentNodeIndex;
                if (parentNodeIndex != -1
                    && points.Count > 0
                    && Math.Sign(currentPoint.X - previousPoint.X) == Math.Sign(previousPoint.X - beforePreviousPoint.X)
                    && Math.Sign(currentPoint.Y - previousPoint.Y) == Math.Sign(previousPoint.Y - beforePreviousPoint.Y))
                {
                    // The path isn't changing directions, just override the last point.
                    points[points.Count - 1] = currentPoint;
                }
                else
                {
                    points.Add(currentPoint);
                    if (parentNodeIndex == -1) break;
                }

                currentPointIndex = parentNodeIndex;
                beforePreviousPoint = previousPoint;
                previousPoint = currentPoint;
                currentPoint = IndexToPoint(currentPointIndex);
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

        private void Open(Point nodePoint, int parentNodeIndex, float costFromSource)
        {
            int nodeIndex = PointToIndex(nodePoint);

            float distanceToDestination = destinationDistanceEvaluator(nodePoint);

            PathNode node = new PathNode(parentNodeIndex, costFromSource, distanceToDestination);
            nodes[nodeIndex] = node;
            nodeStates[nodeIndex] = openNodeStateValue;
            openList.Add(nodeIndex);

            if (nodeNearestToDestinationIndex == -1
                || distanceToDestination < nodes[nodeNearestToDestinationIndex].DistanceToDestination)
            {
                nodeNearestToDestinationIndex = nodeIndex;
            }

            ++visitedNodeCount;
        }

        private void Close(int nodeIndex)
        {
            Debug.Assert(nodeStates[nodeIndex] == openNodeStateValue);
            openList.Remove(nodeIndex);
            nodeStates[nodeIndex] = closedNodeStateValue;
        }

        private bool IsOpenable(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= gridSize.Width || point.Y >= gridSize.Height)
                return false;

            int nodeIndex = PointToIndex(point);
            return nodeStates[nodeIndex] != closedNodeStateValue && isWalkable(point);
        }
        #endregion

        private int FindPathNodes()
        {
            Point startPoint = IndexToPoint(startPointIndex);
            Open(startPoint, -1, 0);
            while (true)
            {
                int currentNodeIndex = openList.Cheapest;
                PathNode currentNode = nodes[currentNodeIndex];
                if (currentNode.IsDestination) return currentNodeIndex;

                Close(currentNodeIndex);

                Point currentNodePoint = IndexToPoint(currentNodeIndex);
                AddNearbyNodes(currentNodePoint);

                if (openList.Count == 0 || visitedNodeCount >= maxNodesToVisit)
                    return nodeNearestToDestinationIndex;
            }
        }

        private static float GetDistance(Point a, Point b)
        {
            return ((Vector2)a - (Vector2)b).LengthFast;
        }

        #region Adding Nodes
        private void AddNearbyNodes(Point currentNodePoint)
        {
            AddDiagonalNode(currentNodePoint, -1, -1);
            AddSideNode(currentNodePoint, 0, -1);
            AddDiagonalNode(currentNodePoint, 1, -1);
            AddSideNode(currentNodePoint, -1, 0);
            AddSideNode(currentNodePoint, 1, 0);
            AddDiagonalNode(currentNodePoint, -1, 1);
            AddSideNode(currentNodePoint, 0, 1);
            AddDiagonalNode(currentNodePoint, 1, 1);
        }

        private void AddSideNode(Point currentNodePoint, int offsetX, int offsetY)
        {
            Point adjacentPoint = new Point(currentNodePoint.X + offsetX, currentNodePoint.Y + offsetY);
            AddAdjacentNode(currentNodePoint, adjacentPoint, sideMovementCost);
        }

        private void AddDiagonalNode(Point currentNodePoint, int offsetX, int offsetY)
        {
            // Disallow going from A to B in situations like (# is non-walkable):
            ///
            // #B
            // A#
            Point adjacentPoint = new Point(currentNodePoint.X + offsetX, currentNodePoint.Y + offsetY);
            if (!IsOpenable(new Point(adjacentPoint.X, currentNodePoint.Y))
                || !IsOpenable(new Point(currentNodePoint.X, adjacentPoint.Y)))
                return;

            AddAdjacentNode(currentNodePoint, adjacentPoint, diagonalMovementCost);
        }

        private void AddAdjacentNode(Point currentNodePoint, Point adjacentPoint, float movementCost)
        {
            if (!IsOpenable(adjacentPoint)) return;

            int currentNodeIndex = PointToIndex(currentNodePoint);
            PathNode currentNode = nodes[currentNodeIndex];

            float costFromSource = currentNode.CostFromSource + movementCost;

            int adjacentNodeIndex = PointToIndex(adjacentPoint);
            if (nodeStates[adjacentNodeIndex] == openNodeStateValue)
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
