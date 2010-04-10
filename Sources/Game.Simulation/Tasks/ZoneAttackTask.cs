using System;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a<see cref="Unit"/> move to a location and attack enemies on it's way.
    /// </summary>
    [Serializable]
    public sealed class ZoneAttackTask : Task
    {
        #region Fields
        private readonly Vector2 destination;
        private readonly float targetDistance;
        private Unit target = null;
        private AttackTask attack = null;
        private MoveTask move;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ZoneAttack"/> task from the <see cref="Unit"/>
        /// that attacks and its destination.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> who attacks.</param>
        /// <param name="destination">The destination of the unit'</param>
        public ZoneAttackTask(Unit unit, Vector2 destination)
            : base(unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            
            this.destination = destination;
            this.targetDistance = unit.GetStat(AttackSkill.RangeStat);
            this.move = new MoveTask(unit, (Point)destination);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "attacking while moving to {0}".FormatInvariant(destination); }
        }

        /// <summary>
        /// Gets the current distance remaining between this <see cref="Unit"/>
        /// and the followed <see cref="Unit"/>.
        /// </summary>
        public float CurrentDistance
        {
            get { return (target.Position - Unit.Position).Length; }
        }

        /// <summary>
        /// Gets a value indicating if the following <see cref="Unit"/>
        /// is within the target range of its <see cref="target"/>.
        /// </summary>
        public bool IsInRange
        {
            get { return CurrentDistance <= targetDistance; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (attack == null && Unit.CanPerformProximityChecks(step))
            {
                Unit target = Unit.World.Entities
                    .Intersecting(Unit.LineOfSight)
                    .OfType<Unit>()
                    .FirstOrDefault(other => Unit.IsInLineOfSight(other)
                        && Unit.Faction.GetDiplomaticStance(other.Faction) == DiplomaticStance.Enemy);

                if (target != null) attack = new AttackTask(Unit, target);
            }

            if (attack == null)
            {
                move.Update(step);
                if (move.HasEnded) MarkAsEnded();
            }
            else
            {
                attack.Update(step);
                if (attack.HasEnded) attack = null;
            }
        }
        #endregion
    }
}
