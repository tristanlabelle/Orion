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
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> Move to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class Move : Task
    {
        #region Fields
        private const float angularVelocity = (float)Math.PI * 2;

        private readonly Vector2 destination;
        private PathWalker pathWalker;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Move"/> task from the <see cref="Unit"/>
        /// that gets Moved and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be Moved.</param>
        /// <param name="destination">The location of the unit'</param>
        public Move(Unit unit, Vector2 destination)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.Move>())
                throw new ArgumentException("Cannot Move without the Move skill.", "unit");
            if (unit.IsAirborne) throw new ArgumentException("Airborne units cannot Move.", "unit");
            if (!unit.World.IsWithinBounds((Point)destination))
                throw new ArgumentOutOfRangeException("destination");

            this.destination = (Point)destination;
            this.pathWalker = GetPathWalker();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Path"/> this <see cref="Unit"/> uses to get to its destination.
        /// </summary>
        public Path Path
        {
            get { return pathWalker.Path; }
        }

        public override bool HasEnded
        {
            get { return pathWalker == null || pathWalker.HasReachedDestination; }
        }

        public override string Description
        {
            get { return "moving to {0}".FormatInvariant(destination); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(float timeDelta)
        {
            float distance = Unit.GetStat(UnitStat.MovementSpeed) * timeDelta;
            pathWalker.Walk(distance);

            Region targetRegion = Entity.GetGridRegion(pathWalker.Position, Unit.Size);
            if (Unit.Type.IsAirborne || CanMoveOn(targetRegion))
            {
                Unit.Angle = pathWalker.Angle;
                Unit.SetPosition(pathWalker.Position);
            }
            else
            {
                // An obstacle is blocking us
                Unit.SetPosition(Unit.GridRegion.Min);
                pathWalker = GetPathWalker();
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
            return new PathWalker(GetPath());
        }

        private Path GetPath()
        {
            if (Unit.Type.IsAirborne)
            {
                List<Point> points = new List<Point>();
                points.Add((Point)Unit.Position);
                points.Add((Point)destination);
                return new Path(points);
            }
            else
            {
                return Unit.Faction.FindPath(Unit.Position, point => ((Vector2)point - destination).LengthFast);
            }
        }
        #endregion
    }
}
        