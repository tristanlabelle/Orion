using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Pathfinding
{
    public sealed class Pathfinder
    {
        #region Fields
        private static readonly float[] movementCostFromSteps = new[] { 0f, 1f, (float)Math.Sqrt(2) };

        private readonly World world;
        private readonly Pool<PathNode> nodePool = new Pool<PathNode>();
        private readonly Dictionary<Point16, PathNode> openNodes = new Dictionary<Point16, PathNode>();
        private readonly Dictionary<Point16, PathNode> closedNodes = new Dictionary<Point16, PathNode>();
        private readonly List<Vector2> points = new List<Vector2>();
        private Point16 destinationPoint;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Pathfinder"/> that from the <see cref="World"/> in which it operates.
        /// </summary>
        /// <param name="world">The <see cref="World"/> in which paths are to be found.</param>
        public Pathfinder(World world)
        {
            Argument.EnsureNotNull(world, "world");
            this.world = world;
        }
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
        public Path FindPath(Vector2 source, Vector2 destination)
        {
            if (!world.Bounds.ContainsPoint(source))
                throw new ArgumentOutOfRangeException("The source of the path is out of world bounds.", "source");
            
            destination = world.Bounds.ClosestPointInside(destination);

            Point16 sourcePoint = world.GetClampedTileCoordinates(source);
            destinationPoint = world.GetClampedTileCoordinates(destination);

            if (!world.Terrain.IsWalkable(destinationPoint))
            {
                // Abandon finding a path if we know it's going to fail.
                // Hopefully this can be removed once more optimizations are in.
                return null;
            }

            CleanUp();

            PathNode destinationNode = FindPathNodes(sourcePoint);

            if (destinationNode == null) destinationNode = FindClosedNodeNearestToDestination();

            FindPathPointsTo(destinationNode);
            OptimizePathPoints();
            return new Path(world, source, destination, points);
        }

        private void OptimizePathPoints()
        {
            for (int i = 0; i < points.Count - 2; ++i)
            {
                Vector2 sourcePoint = points[i];
                while (i != points.Count - 2)
                {
                    Vector2 destinationPoint = points[i + 2];
                    LineSegment lineSegment = new LineSegment(sourcePoint, destinationPoint);
                    if (!world.Terrain.IsWalkable(lineSegment, 1))
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
            return closedNodes
                .OrderBy(node => node.Value.EstimatedCostToDestination)
                .Select(node => node.Value)
                .FirstOrDefault();
        }

        private void FindPathPointsTo(PathNode destinationNode)
        {
            PathNode currentNode = destinationNode;
            while (currentNode != null)
            {
                points.Add(currentNode.Position + new Vector2(0.5f, 0.5f));
                currentNode = currentNode.ParentNode;
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
            while (currentNode.Position.X != destinationPoint.X || currentNode.Position.Y != destinationPoint.Y)
            {
                closedNodes.Add(currentNode.Position, currentNode);
                openNodes.Remove(currentNode.Position);
                AddNearbyNodes(currentNode);

                if (openNodes.Count == 0)
                    return null;

                currentNode = GetCheapestOpenNode();
            }

            return currentNode;
        }

        private PathNode GetCheapestOpenNode()
        {
            PathNode cheapestNode = openNodes.First().Value;
            foreach (PathNode openNode in openNodes.Values)
                if (openNode.TotalCost < cheapestNode.TotalCost)
                    cheapestNode = openNode;
            return cheapestNode;
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

        private void AddDiagonalAdjacentNode(PathNode currentNode, short offsetX, short offsetY)
        {
            if (!IsOpenable(new Point16((short)(currentNode.Position.X + offsetX), currentNode.Position.Y))
                || !IsOpenable(new Point16(currentNode.Position.X, (short)(currentNode.Position.Y + offsetY))))
                return;

            AddAdjacentNode(currentNode, offsetX, offsetY);
        }

        private void AddAdjacentNode(PathNode currentNode, short offsetX, short offsetY)
        {
            int x = currentNode.Position.X + offsetX;
            int y = currentNode.Position.Y + offsetY;
            Point16 nearNode = new Point16((short)x, (short)y);
            AddNearbyNode(currentNode, nearNode);
        }

        private bool IsOpenable(Point16 nearbyPoint)
        {
            return world.IsWithinBounds(nearbyPoint)
                && !closedNodes.ContainsKey(nearbyPoint)
                && world.Terrain.IsWalkable(nearbyPoint);
        }

        private void AddNearbyNode(PathNode currentNode, Point16 nearbyPoint)
        {
            if (!IsOpenable(nearbyPoint)) return;

            float movementCost = GetMovementCost(currentNode.Position, nearbyPoint);
            float costFromSource = currentNode.CostFromSource + movementCost;

            PathNode nearbyNode;
            if (openNodes.TryGetValue(nearbyPoint, out nearbyNode))
            {
                if (costFromSource < nearbyNode.CostFromSource)
                {
                    // If it is a better choice to pass through the current node, overwrite the parent and the move cost
                    float estimatedCostToDestination = GetMovementCost(nearbyPoint, destinationPoint);
                    nearbyNode.SetCosts(currentNode, costFromSource, estimatedCostToDestination);
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
        #endregion

        #region Properties
        public World World
        {
            get { return world; }
        }
        #endregion
    }
}
