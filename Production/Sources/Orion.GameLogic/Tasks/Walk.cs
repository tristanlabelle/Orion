using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OpenTK.Math;

using Orion.GameLogic.Pathfinding;
using System.Diagnostics;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/>, which causes a <see cref="Unit"/>
    /// to walk on the ground to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class Walk : Move
    {
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
        public Walk(Unit unit, Func<Point, float> destinationDistanceEvaluator)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.Move>())
                throw new ArgumentException("Cannot walk without the move skill.", "unit");
            if (unit.IsAirborne) throw new ArgumentException("Airborne units cannot walk.", "unit");
            Argument.EnsureNotNull(destinationDistanceEvaluator, "destinationDistanceEvaluator");

            this.destinationDistanceEvaluator = destinationDistanceEvaluator;
            this.pathWalker = GetPathWalker();
        }

        public Walk(Unit unit, Point destination)
            : this(unit, (point => ((Vector2)point - (Vector2)destination).LengthFast)) { }
        #endregion

        #region Properties
        public override Vector2 Destination
        {
            get { return pathWalker.Path.Destination; }
        }

        /// <summary>
        /// Gets the <see cref="Path"/> this <see cref="Unit"/> uses to get to its destination.
        /// </summary>
        public override Path Path
        {
            get { return pathWalker.Path; }
        }

        public override bool HasEnded
        {
            get { return pathWalker == null ? attemptCount >= maxAttemptCount : pathWalker.HasReachedDestination; }
        }

        public override string Description
        {
            get { return "walking to {0}".FormatInvariant(Path.Destination); }
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
                if (!Unit.World.Terrain.IsWalkable(point)) return false;
                Entity entity = Unit.World.Entities.GetSolidEntityAt(point);
                if (entity != null && entity != Unit) return false;
            }
            return true;
        }

        private PathWalker GetPathWalker()
        {
            Path path = Unit.Faction.FindPath(Unit.GridRegion.Min, destinationDistanceEvaluator);
            if (path.Source == path.Destination) return null;
            return new PathWalker(path);
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
    }
}
