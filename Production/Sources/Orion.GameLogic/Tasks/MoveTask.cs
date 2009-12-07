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
    /// A <see cref="Task"/>, which makes a <see cref="Unit"/> move to a specified destination.
    /// </summary>
    [Serializable]
    public abstract class MoveTask : Task
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
        protected MoveTask(Unit unit)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (!unit.HasSkill<Skills.MoveSkill>())
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

        /// <summary>
        /// Gets a value indicating if the <see cref="Unit"/> has reached its destiation.
        /// </summary>
        /// <remarks>
        /// Moving tasks can fail and end without having reached any destination if the path if blocked.
        /// </remarks>
        public abstract bool HasReachedDestination { get; }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static MoveTask ToPoint(Unit unit, Vector2 destination)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (unit.IsAirborne) return new FlyTask(unit, destination);
            return new WalkTask(unit, (Point)destination);
        }

        public static MoveTask ToNearRegion(Unit unit, Region region)
        {
            Argument.EnsureNotNull(unit, "unit");

            if (unit.IsAirborne)
            {
                // Fly to the center of the region
                return new FlyTask(unit, region.Min + (Vector2)region.Size * 0.5f);
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
                return new WalkTask(unit, destinationDistanceEvaluator);
            }
        }
        #endregion
        #endregion
    }
}
        