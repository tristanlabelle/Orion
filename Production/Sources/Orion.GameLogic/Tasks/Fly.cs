using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.GameLogic.Pathfinding;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> causing a unit to fly to a destination.
    /// </summary>
    public sealed class Fly : Move
    {
        #region Fields
        private readonly Vector2 destination;
        #endregion

        #region Constructors
        public Fly(Unit unit, Vector2 destination)
            : base(unit)
        {
            if (!unit.IsAirborne) throw new ArgumentException("Only airborne unit may fly.", "unit");
            if (!unit.World.IsWithinBounds((Point)destination))
                throw new ArgumentOutOfRangeException("destination");
            this.destination = destination;
            Unit.LookAt(destination + new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Properties
        public override Vector2 Destination
        {
            get { return destination; }
        }

        public override Path Path
        {
            get { return new Path(new[] { (Point)Source, (Point)destination }); }
        }

        public override bool HasEnded
        {
            get { return Unit.Position == destination; }
        }

        public override string Description
        {
            get { return "Flying to {0}.".FormatInvariant(destination); }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(UpdateInfo info)
        {
            float distance = Unit.GetStat(UnitStat.MovementSpeed) * info.TimeDeltaInSeconds;
            Vector2 delta = destination - Unit.Position;
            if (distance > delta.LengthFast)
            {
                Unit.SetPosition(destination);
                return;
            }

            Unit.SetPosition(Unit.Position + Vector2.Normalize(delta) * distance);
        }
        #endregion
    }
}
