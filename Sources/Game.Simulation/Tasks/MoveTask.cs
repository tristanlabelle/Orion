using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Pathfinding;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
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
        private const float maxPathingFailureTime = 1.3f;
        private const float timeBetweenRepathings = 0.4f;

        private readonly Func<Point, float> destinationDistanceEvaluator;
        private Path path;
        private int targetPathPointIndex;
        private float timeSinceLastPathing = float.PositiveInfinity;
        private float timeSinceLastSuccessfulPathing;
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
            if (!unit.HasSkill<MoveSkill>())
                throw new ArgumentException("Cannot move without the move skill.", "unit");
            Argument.EnsureNotNull(destinationDistanceEvaluator, "destinationDistanceEvaluator");

            Debug.Assert(unit.IsAirborne || unit.Size.Area == 1, "Ground units bigger than 1x1 are not supported.");

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
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
            get { return path; }
        }

        public override bool HasEnded
        {
            get { return path == null ? timeSinceLastSuccessfulPathing >= maxPathingFailureTime : HasReachedDestination; }
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
            timeSinceLastPathing += step.TimeDeltaInSeconds;
            timeSinceLastSuccessfulPathing += step.TimeDeltaInSeconds;

            bool needsNewPath = (path == null || targetPathPointIndex == path.PointCount);
            if (needsNewPath && !Repath()) return;

            float distance = Unit.GetStat(MoveSkill.SpeedStat) * step.TimeDeltaInSeconds;

            Vector2 targetPathPoint = path.Points[targetPathPointIndex];
            Unit.LookAt(targetPathPoint + (Vector2)Unit.Size * 0.5f);

            Vector2 deltaToPathPoint = targetPathPoint - Unit.Position;

            Vector2 targetPosition;
            if (distance > deltaToPathPoint.LengthFast)
            {
                targetPosition = targetPathPoint;
                ++targetPathPointIndex;
            }
            else
            {
                targetPosition = Unit.Position + Vector2.Normalize(deltaToPathPoint) * distance;
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
                Repath();
            }
        }

        private bool CanMoveOn(Region targetRegion)
        {
            foreach (Point point in targetRegion.Points)
            {
                if (!Unit.IsAirborne && !World.Terrain.IsWalkable(point)) return false;
                Entity entity = World.Entities.GetEntityAt(point, Unit.CollisionLayer);
                if (entity != null && entity != Unit) return false;
            }

            return true;
        }

        private bool Repath()
        {
            path = null;
            if (timeSinceLastPathing < timeBetweenRepathings)
                return false;

            path = Unit.World.FindPath(Unit.GridRegion.Min, destinationDistanceEvaluator, GetWalkabilityTester());
            targetPathPointIndex = (path.PointCount > 1) ? 1 : 0;
            timeSinceLastPathing = 0;

            if (!path.IsComplete && path.Source == path.End)
                return false;

            timeSinceLastSuccessfulPathing = 0;

            return true;
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
            if (!Unit.Faction.HasSeen(point)) return true;
            return Unit.World.Terrain.IsWalkable(point) && World.Entities.GetGroundEntityAt(point) == null;
        }

        private bool IsAirPathable(Point minPoint)
        {
            Region region = new Region(minPoint, Unit.Size);
            if (region.ExclusiveMaxX > World.Size.Width || region.ExclusiveMaxY > World.Size.Height)
                return false;

            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
            {
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                {
                    Point occupied = new Point(x, y);
                    Entity entity = Unit.World.Entities.GetAirEntityAt(occupied);
                    if (entity != null && entity != Unit) return false;
                }
            }

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
