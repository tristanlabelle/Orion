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
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> move to a specified destination.
    /// </summary>
    [Serializable]
    public sealed class Move : Task
    {
        #region Fields
        private const float angularVelocity = (float)Math.PI * 2;

        private readonly Unit unit;
        private readonly Vector2 destination;
        private Path path;
        private int nextPointIndex = 1;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Move"/> task from the <see cref="Unit"/>
        /// that gets moved and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be moved.</param>
        /// <param name="destination">The location of the unit'</param>
        public Move(Unit unit, Vector2 destination)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.Move>())
                throw new ArgumentException("Cannot move without the move skill.", "unit");
            if (!unit.World.IsWithinBounds((Point)destination))
                throw new ArgumentOutOfRangeException("destination");

            this.unit = unit;
            this.destination = (Point)destination;
            this.path = FindPathToDestination();
        }
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
            get { return path == null || nextPointIndex > path.Points.Count - 1; }
        }

        public override string Description
        {
            get { return "moving to {0}".FormatInvariant(destination); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(float timeDelta)
        {
            if (HasEnded) return;

            Vector2 targetPathPoint = path.Points[nextPointIndex];
            Vector2 delta = targetPathPoint - unit.Position;
            Vector2 direction = Vector2.Normalize(delta);

            unit.Angle = (float)Math.Atan2(direction.Y, direction.X);

            float distance = unit.GetStat(UnitStat.MovementSpeed) * timeDelta;
            Vector2 targetPosition;
            if (distance < delta.Length)
            {
                targetPosition = unit.Position + (direction * distance);
            }
            else
            {
                // Unit will reach destination within this frame
                targetPosition = targetPathPoint;
                ++nextPointIndex;
            }

            Region targetRegion = Entity.GetGridRegion(targetPosition, unit.Size);
            if (unit.Type.IsAirborne || CanWalkOn(targetRegion))
            {
                unit.SetPosition(targetPosition);
            }
            else
            {
                // An obstacle is blocking us
                unit.SetPosition(unit.GridRegion.Min);
                path = unit.Faction.FindPath(unit.Position, destination);
                nextPointIndex = 1;
            }
        }

        private bool CanWalkOn(Region targetRegion)
        {
            for (int x = targetRegion.MinX; x < targetRegion.ExclusiveMaxX; ++x)
            {
                for (int y = targetRegion.MinY; y < targetRegion.ExclusiveMaxY; ++y)
                {
                    Point point = new Point(x, y);
                    if (!unit.World.Terrain.IsWalkable(point)) return false;
                    Entity entity = unit.World.Entities.GetSolidEntityAt(point);
                    if (entity != null && entity != unit) return false;
                }
            }
            return true;
        }
        private Path FindPathToDestination()
        {
            if (unit.Type.IsAirborne)
            {
                List<Vector2> points = new List<Vector2>();
                points.Add(unit.Position);
                points.Add(destination);
                return new Path(unit.Position, destination, points);
            }
            else
            {
                return unit.Faction.FindPath(unit.Position, destination);
            }
        }
        #endregion
    }
}
        