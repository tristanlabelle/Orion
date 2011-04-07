﻿using System;
using System.Diagnostics;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Pathfinding;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which causes an <see cref="Entity"/>
    /// to walk on the ground to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class MoveTask : Task
    {
        #region Instance
        #region Fields
        private static readonly TimeSpan maxPathingFailureTime = TimeSpan.FromSeconds(1.3);
        private static readonly TimeSpan timeBetweenRepathings = TimeSpan.FromSeconds(0.4);

        private readonly Func<Point, float> destinationDistanceEvaluator;
        private Path path;
        private int targetPathPointIndex;
        private TimeSpan timeSinceLastPathing = TimeSpan.MaxValue;
        private TimeSpan timeSinceLastSuccessfulPathing;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Walk"/> task from the <see cref="Entity"/>
        /// that gets moved and a delegate to find its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Entity"/> to be moved.</param>
        /// <param name="destinationDistanceEvaluator">
        /// A delegate to a method which evaluates the distance of tiles to the destination.
        /// </param>
        public MoveTask(Entity entity, Func<Point, float> destinationDistanceEvaluator)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "unit");
            if (!entity.Components.Has<Mobile>())
                throw new ArgumentException("Cannot move without the move skill.", "unit");
            Argument.EnsureNotNull(destinationDistanceEvaluator, "destinationDistanceEvaluator");

            Debug.Assert(entity.Components.Has<Spatial>(), "Unit has no spatial component!");
            Debug.Assert(entity.Spatial.CollisionLayer == CollisionLayer.Air || entity.Size.Area == 1, "Ground units bigger than 1x1 are not supported.");

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
        }

        public MoveTask(Entity entity, Point destination)
            : this(entity, (point => ((Vector2)point - (Vector2)destination).LengthFast)) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Path"/> this <see cref="Entity"/> uses to get to its destination.
        /// </summary>
        public Path Path
        {
            get { return path; }
        }

        public bool HasReachedDestination
        {
            get { return path != null && path.IsComplete && targetPathPointIndex == path.PointCount; }
        }

        public override string Description
        {
            get
            {
                if (path == null) return "moving";
                return "moving to " + path.End.ToStringInvariant();
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            if (spatial == null || !Entity.Components.Has<Mobile>())
            {
                MarkAsEnded();
                return;
            }

            timeSinceLastPathing += step.TimeDelta;
            timeSinceLastSuccessfulPathing += step.TimeDelta;

            if (HasReachedDestination
                || (path == null && timeSinceLastSuccessfulPathing >= maxPathingFailureTime))
            {
                MarkAsEnded();
                return;
            }

            bool needsNewPath = (path == null || targetPathPointIndex == path.PointCount);
            if (needsNewPath && !Repath(spatial)) return;

            float distance = (float)Entity.GetStatValue(Mobile.SpeedStat) * step.TimeDeltaInSeconds;

            Vector2 targetPathPoint = path.Points[targetPathPointIndex];
            spatial.LookAt(targetPathPoint + (Vector2)spatial.Size * 0.5f);

            Vector2 deltaToPathPoint = targetPathPoint - spatial.Position;

            Vector2 targetPosition;
            if (distance > deltaToPathPoint.LengthFast)
            {
                targetPosition = targetPathPoint;
                ++targetPathPointIndex;
            }
            else
            {
                targetPosition = spatial.Position + Vector2.Normalize(deltaToPathPoint) * distance;
            }

            // Prevents floating point inaccuracies, we've had values of -0.0000001f
            targetPosition = World.Bounds.Clamp(targetPosition);

            Region targetRegion = Entity.GetGridRegion(targetPosition, spatial.Size);
            if (CanMoveOn(spatial, targetRegion))
            {
                spatial.Position = targetPosition;
            }
            else
            {
                // An obstacle is blocking us
                Repath(spatial);
            }
        }

        private bool CanMoveOn(Spatial spatial, Region targetRegion)
        {
            foreach (Point point in targetRegion.Points)
            {
                Debug.Assert(Entity.Components.Has<Spatial>(), "Unit has no spatial component!");
                if (Entity.Spatial.CollisionLayer == CollisionLayer.Ground
                    && !World.Terrain.IsWalkable(point)) return false;

                Entity entity = World.Entities.GetEntityAt(point, spatial.CollisionLayer);
                if (entity != null && entity != Entity) return false;
            }

            return true;
        }

        private bool Repath(Spatial spatial)
        {
            path = null;
            if (timeSinceLastPathing < timeBetweenRepathings)
                return false;

            path = Entity.World.FindPath(spatial.GridRegion.Min, destinationDistanceEvaluator, GetWalkabilityTester());
            targetPathPointIndex = (path.PointCount > 1) ? 1 : 0;
            timeSinceLastPathing = TimeSpan.Zero;

            if (!path.IsComplete && path.Source == path.End)
                return false;

            timeSinceLastSuccessfulPathing = TimeSpan.Zero;

            return true;
        }

        private Func<Point, bool> GetWalkabilityTester()
        {
            CollisionLayer layer = Entity.Spatial.CollisionLayer;
            if (layer == CollisionLayer.Ground) return IsGroundPathable;
            if (layer == CollisionLayer.Air) return IsAirPathable;
            throw new InvalidOperationException(
                "Cannot pathfind for a unit on collision layer {0}.".FormatInvariant(Entity));
        }

        private bool IsGroundPathable(Point point)
        {
            Faction faction = FactionMembership.GetFaction(Entity);
            if (faction != null && !faction.HasSeen(point)) return true;
            return Entity.World.Terrain.IsWalkable(point) && World.Entities.GetGroundEntityAt(point) == null;
        }

        private bool IsAirPathable(Point minPoint)
        {
            Spatial spatial = Entity.Spatial;
            Region region = new Region(minPoint, spatial.Size);
            if (region.ExclusiveMaxX > World.Size.Width || region.ExclusiveMaxY > World.Size.Height)
                return false;

            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                {
                    Point occupied = new Point(x, y);
                    Entity entity = World.Entities.GetAirEntityAt(occupied);
                    if (entity != null && entity != Entity) return false;
                }
            }

            return true;
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static MoveTask ToNearRegion(Entity entity, Region region)
        {
            Argument.EnsureNotNull(entity, "entity");

            // Walk to a tile surrounding the region
            Region grownRegion = Region.Grow(region, 1);
            Func<Point, float> destinationDistanceEvaluator = point =>
            {
                Debug.Assert(entity.Components.Has<Spatial>(), "Unit has no spatial component!");
                if (region.Contains(point))
                    return entity.Spatial.CollisionLayer == CollisionLayer.Air ? 0 : 1;

                return ((Vector2)point - (Vector2)grownRegion.Clamp(point)).LengthFast;
            };

            return new MoveTask(entity, destinationDistanceEvaluator);
        }
        #endregion
        #endregion
    }
}
