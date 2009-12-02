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
        private const int maxAttemptCount = 8;

        private readonly Func<Point, float> destinationDistanceEvaluator;
        private bool isOnPath;
        private PathWalker pathWalker;
        private int attemptCount = 1;
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

        public Walk(Unit unit, Vector2 destination)
            : this(unit, (point => ((Vector2)point - destination).LengthFast)) { }
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
            get { return pathWalker == null || pathWalker.HasReachedDestination; }
        }

        public override string Description
        {
            get { return "walking to {0}".FormatInvariant(Path.Destination); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(float timeDelta)
        {
            float distance = Unit.GetStat(UnitStat.MovementSpeed) * timeDelta;

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

            Region targetRegion = Entity.GetGridRegion(targetPosition, Unit.Size);
            if (CanMoveOn(targetRegion))
            {
                Unit.SetPosition(targetPosition);
            }
            else
            {
                // An obstacle is blocking us
                if (attemptCount >= maxAttemptCount)
                {
                    pathWalker = null;
                }
                else
                {
                    pathWalker = GetPathWalker();
                    isOnPath = false;
                    ++attemptCount;
                }
            }
        }

        private bool CanMoveOn(Region targetRegion)
        {
            for (int x = targetRegion.MinX; x < targetRegion.ExclusiveMaxX; ++x)
            {
                for (int y = targetRegion.MinY; y < targetRegion.ExclusiveMaxY; ++y)
                {
                    Point point = new Point(x, y);
                    if (!Unit.World.Terrain.IsWalkable(point)) return false;
                    Entity entity = Unit.World.Entities.GetSolidEntityAt(point);
                    if (entity != null && entity != Unit) return false;
                }
            }
            return true;
        }

        private PathWalker GetPathWalker()
        {
            Path path = Unit.Faction.FindPath((Point)Unit.Center, destinationDistanceEvaluator);
            return new PathWalker(path);
        }
        #endregion
    }
}
