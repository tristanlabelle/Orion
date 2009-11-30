using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A task causing a unit to fly to a destination.
    /// </summary>
    public sealed class Fly : Task
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
        }
        #endregion

        #region Properties
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
        protected override void DoUpdate(float timeDelta)
        {
            Vector2 delta = destination - Unit.Position;
            Unit.Angle = (float)Math.Atan2(delta.Y, delta.X);
            float distance = Unit.GetStat(UnitStat.MovementSpeed) * timeDelta;
            if (delta.LengthFast > distance) Unit.SetPosition(destination);
            else Unit.SetPosition(Vector2.Normalize(delta) * distance);
        }
        #endregion
    }
}
