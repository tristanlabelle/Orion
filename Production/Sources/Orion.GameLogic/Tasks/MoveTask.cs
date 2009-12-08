using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which causes a <see cref="Unit"/>
    /// to walk on the ground to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class MoveTask : Task
    {
        #region Instance
        #region Fields
        private const int maxAttemptCount = 20;
        private const float timeBeforeRepathfinding = 0.5f;

        private readonly Func<Point, float> destinationDistanceEvaluator;
        private bool isOnPath;
        private PathWalker pathWalker;
        private int attemptCount = 1;
        private float timeSinceLastPathfinding;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Walk"/> task from the <see cref="Unit"/>
        /// that gets moved and a delegate to find its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be moved.</param>
        /// <param name="destinationDistanceEvaluator">
        /// A delegate to a method which evaluates the distance of tiles to the destination.
        /// </param>
        public MoveTask(Unit unit, Func<Point, float> destinationDistanceEvaluator)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.MoveSkill>())
                throw new ArgumentException("Cannot walk without the move skill.", "unit");
            Argument.EnsureNotNull(destinationDistanceEvaluator, "destinationDistanceEvaluator");

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
            this.pathWalker = GetPathWalker();
        }

        public MoveTask(Unit unit, Point destination)
            : this(unit, (point => ((Vector2)point - (Vector2)destination).LengthFast)) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Path"/> this <see cref="Unit"/> uses to get to its destination.
        /// </summary>
        public Path Path
        {
            get { return pathWalker == null ? null : pathWalker.Path; }
        }

        public override bool HasEnded
        {
            get { return pathWalker == null ? attemptCount >= maxAttemptCount : pathWalker.HasReachedEnd; }
        }

        public bool HasReachedDestination
        {
            get { return pathWalker != null && pathWalker.HasReachedEnd && pathWalker.Path.IsComplete; }
        }

        public override string Description
        {
            get
            {
                if (pathWalker == null) return "walking";
                return "walking to {0}".FormatInvariant(pathWalker.Path.End);
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(UpdateInfo info)
        {
            timeSinceLastPathfinding += info.TimeDeltaInSeconds;

            if (pathWalker == null && !TryRepath()) return;

            float distance = Unit.GetStat(UnitStat.MovementSpeed) * info.TimeDeltaInSeconds;

            Vector2 targetPosition;
            if (isOnPath)
            {
                pathWalker.Walk(distance);
                targetPosition = pathWalker.Position;
                Unit.Angle = pathWalker.Angle;
            }
            else
            {
                // Get on the path before following it
                Vector2 deltaToPathSource = pathWalker.Path.Source - Unit.Position;
                if (distance > deltaToPathSource.LengthFast)
                {
                    targetPosition = pathWalker.Path.Source;
                    isOnPath = true;
                }
                else
                {
                    targetPosition = Unit.Position + Vector2.Normalize(deltaToPathSource) * distance;
                }
            }

            // Prevents floating point inaccuracies, we've had values of -0.0000001f
            targetPosition = Unit.World.Bounds.Clamp(targetPosition);

            Region targetRegion = Entity.GetGridRegion(targetPosition, Unit.Size);
            if (CanMoveOn(targetRegion))
            {
                Unit.SetPosition(targetPosition);
            }
            else
            {
                // An obstacle is blocking us
                pathWalker = null;
                TryRepath();
            }
        }

        private bool CanMoveOn(Region targetRegion)
        {
            foreach (Point point in targetRegion.Points)
            {
                if (!Unit.IsAirborne && !Unit.World.Terrain.IsWalkable(point)) return false;
                Entity entity = Unit.World.Entities.GetEntityAt(point, Unit.CollisionLayer);
                if (entity != null && entity != Unit) return false;
            }
            return true;
        }

        private PathWalker GetPathWalker()
        {
            Path path = Unit.World.FindPath(Unit.GridRegion.Min,
                destinationDistanceEvaluator, GetWalkabilityTester());
            if (path.Source == path.End && !path.IsComplete) return null;
            return new PathWalker(path);
        }

        private Func<Point, bool> GetWalkabilityTester()
        {
            if (Unit.CollisionLayer == CollisionLayer.Ground) return IsGroundPathable;
            if (Unit.CollisionLayer == CollisionLayer.Air) return IsAirPathable;
            throw new InvalidOperationException(
                "Cannot pathfind for a unit on collision layer {0}.".FormatInvariant(Unit));
        }

        private bool IsGroundPathable(Point point)
        {
            if (!Unit.World.IsWithinBounds(point)) return false;
            if (Unit.Faction.GetTileVisibility(point) == TileVisibility.Undiscovered) return true;
            return Unit.World.IsFree(point, Unit.CollisionLayer);
        }

        private bool IsAirPathable(Point point)
        {
            Region region = new Region(point, Unit.Size);
            if (region.MinX < 0 || region.MinY < 0
                || region.ExclusiveMaxX > World.Size.Width
                || region.ExclusiveMaxY > World.Size.Height)
                return false;

            foreach (Point regionPoint in region.Points)
            {
                Entity entity = Unit.World.Entities.GetEntityAt(regionPoint, CollisionLayer.Air);
                if (entity != null && entity != Unit) return false;
            }

            return true;
        }

        private bool TryRepath()
        {
            if (timeSinceLastPathfinding < timeBeforeRepathfinding) return false;

            pathWalker = GetPathWalker();
            timeSinceLastPathfinding = 0;
            ++attemptCount;
            if (pathWalker == null) return false;

            isOnPath = false;

            return true;
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static MoveTask ToNearRegion(Unit unit, Region region)
        {
            Argument.EnsureNotNull(unit, "unit");

            // Walk to a tile surrounding the region
            Region grownRegion = Region.Grow(region, 1);
            Func<Point, float> destinationDistanceEvaluator = point =>
            {
                if (region.Contains(point)) return unit.IsAirborne ? 0 : 1;
                return ((Vector2)point - (Vector2)grownRegion.Clamp(point)).LengthFast;
            };

            return new MoveTask(unit, destinationDistanceEvaluator);
        }
        #endregion
        #endregion
    }
}
