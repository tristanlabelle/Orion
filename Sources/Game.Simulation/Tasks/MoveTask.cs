using System;
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
        private TimeSpan timeSinceLastPathing;
        private TimeSpan timeSinceLastSuccessfulPathing;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Walk"/> task from the <see cref="Entity"/>
        /// that gets moved and a delegate to find its destination.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be moved.</param>
        /// <param name="destinationDistanceEvaluator">
        /// A delegate to a method which evaluates the distance of tiles to the destination.
        /// </param>
        public MoveTask(Entity entity, Func<Point, float> destinationDistanceEvaluator)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            if (!entity.Components.Has<Mobile>())
                throw new ArgumentException("Cannot move without the move skill.", "entity");
            Argument.EnsureNotNull(destinationDistanceEvaluator, "destinationDistanceEvaluator");

            Debug.Assert(entity.Components.Has<Spatial>(), "Entity has no spatial component!");
            Debug.Assert(entity.Spatial.CollisionLayer != CollisionLayer.Ground || entity.Spatial.Size.Area == 1,
                "Ground entities bigger than 1x1 are not supported.");

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
            this.timeSinceLastPathing = timeBetweenRepathings;
            this.timeSinceLastSuccessfulPathing = TimeSpan.Zero;
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

        public override Type PublicType
        {
            get { return typeof(MoveTask); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            timeSinceLastPathing += step.TimeDelta;
            timeSinceLastSuccessfulPathing += step.TimeDelta;

            Spatial spatial = Entity.Spatial;
            if (spatial == null || !Entity.Components.Has<Mobile>())
            {
                MarkAsEnded();
                return;
            }

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

            Region targetRegion = Spatial.GetGridRegion(targetPosition, spatial.Size);
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
            Debug.Assert(Entity.Components.Has<Spatial>(), "Unit has no spatial component!");

            int exclusiveMaxX = targetRegion.ExclusiveMaxX;
            int exclusiveMaxY = targetRegion.ExclusiveMaxY;

            for (int y = targetRegion.MinY; y < exclusiveMaxY; ++y)
            {
                for (int x = targetRegion.MinX; x < exclusiveMaxX; ++x)
                {
                    Point point = new Point(x, y);

                    if (Entity.Spatial.CollisionLayer == CollisionLayer.Ground
                        && World.Terrain[point] != TileType.Walkable) return false;

                    Spatial obstacleSpatial = World.SpatialManager.GetGridObstacleAt(point, spatial.CollisionLayer);
                    if (obstacleSpatial != null && obstacleSpatial.Entity != Entity) return false;
                }
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
            if (layer == CollisionLayer.Ground)
            {
                // Tile visibility was previously taken into account here but,
                // as we have so many pathfinding queries,
                // this had a huge impact on performance.

                // Create locals to be captured by the lambda so they do not have to
                // go through the property getters.
                Terrain terrain = World.Terrain;
                SpatialManager spatialManager = World.SpatialManager;
                return point => terrain[point] == TileType.Walkable
                    && spatialManager.GetGroundGridObstacleAt(point.X, point.Y) == null;
            }
            if (layer == CollisionLayer.Air) return IsAirPathable;
            throw new InvalidOperationException(
                "Cannot pathfind for a unit on collision layer {0}.".FormatInvariant(Entity));
        }

        private bool IsGroundPathable(Point point)
        {
            return Entity.World.Terrain[point] == TileType.Walkable
                && World.SpatialManager.GetGroundGridObstacleAt(point.X, point.Y) == null;
        }

        private bool IsAirPathable(Point minPoint)
        {
            Spatial spatial = Entity.Spatial;
            int exclusiveMaxX = minPoint.X + spatial.Width;
            int exclusiveMaxY = minPoint.Y + spatial.Height;
            if (exclusiveMaxX > World.Width || exclusiveMaxY > World.Height)
                return false;

            for (int y = minPoint.Y; y < exclusiveMaxY; ++y)
            {
                for (int x = minPoint.X; x < exclusiveMaxX; ++x)
                {
                    Spatial obstacleSpatial = World.SpatialManager.GetAirGridObstacleAt(x, y);
                    if (obstacleSpatial != null && obstacleSpatial.Entity != Entity) return false;
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
