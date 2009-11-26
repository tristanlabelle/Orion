﻿using System;
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
            this.destination = destination;
            if (unit.Type.IsAirborne)
            {
                List<Vector2> points = new List<Vector2>();
                points.Add(this.unit.Position);
                points.Add(this.destination);
                this.path = new Path(this.unit.Position, this.destination, points);
            }
            else
            {
                this.path = unit.Faction.FindPath(unit.Position, destination);
            }
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
        public override void Update(float timeDelta)
        {
            if (HasEnded) return;

            Vector2 targetPathPoint = path.Points[nextPointIndex];
            Vector2 delta = targetPathPoint - unit.Position;
            Vector2 direction = Vector2.Normalize(delta);

            unit.Angle = (float)Math.Atan2(direction.X, direction.Y);

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

            Rectangle targetBoundingRectangle = new Rectangle(
                targetPosition.X, targetPosition.Y, unit.Size.Width, unit.Size.Height);
            Rectangle targetCollisionRectangle = Entity.GetCollisionRectangle(targetBoundingRectangle);

            if (unit.Type.IsAirborne || unit.World.Terrain.IsWalkable(targetCollisionRectangle))
            {
                // Unit walks along a segment of the path within this frame.
                unit.SetPosition(targetPosition);
            }
            else
            {
                path = unit.Faction.FindPath(unit.Position, destination);
                nextPointIndex = 1;
            }
        }
        #endregion
    }
}
        