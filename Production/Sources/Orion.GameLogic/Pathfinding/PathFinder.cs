using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

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
        private readonly List<Point16> points = new List<Point16>();
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

            try
            {
                PathNode destinationNode = FindPathNodes(sourcePoint);

                if (destinationNode == null) destinationNode = FindClosedNodeNearestToDestination();

                FindPathPointsTo(destinationNode);
                return new Path(world, source, destination, points);
            }
            finally
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
                points.Add(currentNode.Position);
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
            float estimatedCostFromSourceToDestination = EstimateCostToDestination(sourcePoint);
            PathNode sourceNode = GetPathNode(null, sourcePoint, 0, estimatedCostFromSourceToDestination);

            int maxNodesToVisit = world.Width * world.Height / 8;

            PathNode currentNode = sourceNode;
            while (currentNode.Position.X != destinationPoint.X || currentNode.Position.Y != destinationPoint.Y)
            {
                closedNodes.Add(currentNode.Position, currentNode);
                openNodes.Remove(currentNode.Position);
                AddNearbyNodes(currentNode);

                if (closedNodes.Count > maxNodesToVisit || openNodes.Count == 0)
                    return null;

                currentNode = openNodes.First().Value;
                foreach (PathNode openNode in openNodes.Values)
                    if (openNode.TotalCost < currentNode.TotalCost)
                        currentNode = openNode;
            }

            return currentNode;
        }

        private float EstimateCostToDestination(Point16 currentPoint)
        {
            return Math.Abs(currentPoint.X - destinationPoint.X) + Math.Abs(currentPoint.Y - destinationPoint.Y);
        }

        private float GetMovementCost(Point16 a, Point16 b)
        {
            int deltaX = a.X - b.X;
            int deltaY = a.Y - b.Y;
            int stepsAway = Math.Abs(deltaX) + Math.Abs(deltaY);
            return movementCostFromSteps[stepsAway];
        }

        private void AddNearbyNodes(PathNode currentNode)
        {
            AddNearbyNode(currentNode, -1, -1);
            AddNearbyNode(currentNode, 0, -1);
            AddNearbyNode(currentNode, 1, -1);
            AddNearbyNode(currentNode, -1, 0);
            AddNearbyNode(currentNode, 1, 0);
            AddNearbyNode(currentNode, -1, 1);
            AddNearbyNode(currentNode, 0, 1);
            AddNearbyNode(currentNode, 1, 1);
        }

        private void AddNearbyNode(PathNode currentNode, short offsetX, short offsetY)
        {
            int x = currentNode.Position.X + offsetX;
            int y = currentNode.Position.Y + offsetY;
            Point16 nearNode = new Point16((short)x, (short)y);
            AddNearbyNode(currentNode, nearNode);
        }

        private void AddNearbyNode(PathNode currentNode, Point16 nearbyPoint)
        {
            if (!world.IsWithinBounds(nearbyPoint)
                || closedNodes.ContainsKey(nearbyPoint)
                || !world.Terrain.IsWalkable(nearbyPoint))
                return;

            float movementCost = GetMovementCost(currentNode.Position, nearbyPoint);
            float costFromSource = currentNode.CostFromSource + movementCost;

            PathNode nearbyNode;
            if (openNodes.TryGetValue(nearbyPoint, out nearbyNode))
            {
                if (costFromSource < nearbyNode.CostFromSource)
                {
                    // If it is a better choice to pass through the current node, overwrite the parent and the move cost
                    nearbyNode.ParentNode = currentNode;
                    float estimatedCostToDestination = EstimateCostToDestination(nearbyPoint);
                    nearbyNode.SetCosts(costFromSource, estimatedCostToDestination);
                }
            }
            else
            {
                // Add the node to the open list
                float estimatedCostToDestination = EstimateCostToDestination(nearbyPoint);
                nearbyNode = GetPathNode(currentNode, nearbyPoint, costFromSource, estimatedCostToDestination);
                openNodes.Add(nearbyPoint, nearbyNode);
            }
        }
        #endregion

        #region Proprieties
        public World World
        {
            get { return world; }
        }
        #endregion
    }
}
