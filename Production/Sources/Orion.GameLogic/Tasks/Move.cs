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
    public abstract class Move : Task
    {
        #region Instance
        #region Fields
        private readonly Vector2 source;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Move"/> task from the <see cref="Unit"/>
        /// that gets moved and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> to be moved.</param>
        protected Move(Unit unit)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.Move>())
                throw new ArgumentException("Cannot move without the move skill.", "unit");

            this.source = unit.Position;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the source of this move.
        /// </summary>
        public Vector2 Source
        {
            get { return source; }
        }

        /// <summary>
        /// Gets the destination of this move.
        /// </summary>
        public abstract Vector2 Destination { get; }

        /// <summary>
        /// Gets the path followed by this unit.
        /// </summary>
        public abstract Path Path { get; }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static Move ToPoint(Unit unit, Vector2 destination)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (unit.IsAirborne) return new Fly(unit, destination);
            return new Walk(unit, destination);
        }

        public static Move ToNearRegion(Unit unit, Region region)
        {
            Argument.EnsureNotNull(unit, "unit");

            if (unit.IsAirborne)
            {
                // Fly to the center of the region
                return new Fly(unit, region.Min + (Vector2)region.Size * 0.5f);
            }
            else
            {
                // Walk to a tile surrounding the region
                Region grownRegion = Region.Grow(region, 1);
                Func<Point, float> destinationDistanceEvaluator = point =>
                    {
                        if (region.Contains(point)) return 1;
                        return ((Vector2)point - (Vector2)grownRegion.Clamp(point)).LengthFast;
                    };
                return new Walk(unit, destinationDistanceEvaluator);
            }
        }
        #endregion
        #endregion
    }
}
        