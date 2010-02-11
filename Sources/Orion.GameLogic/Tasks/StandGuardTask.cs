using System;
using System.Linq;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes an <see cref="Unit"/> attack enemy units within range, without following.
    /// </summary>
    [Serializable]
    public sealed class StandGuardTask : Task
    {
        #region Fields

        private Unit target;

        #endregion

        #region Constructors

        public StandGuardTask(Unit guard)
            : base(guard)
        {
            Argument.EnsureNotNull(guard, "guard");
        }

        #endregion

        #region Properties

        public Unit Guard
        {
            get { return Unit; }
        }

        public override string Description
        {
            get { return "standing guard"; }
        }

        public override bool HasEnded
        {
            get { return false; }
        }

        #endregion

        #region Methods

        protected override void DoUpdate(SimulationStep step)
        {
            if (Unit.HasSkill<Skills.AttackSkill>())
            {
                if (!IsTargetValid(target))
                    target = Unit.World.Entities
                        .Intersecting(Unit.LineOfSight)
                        .OfType<Unit>()
                        .FirstOrDefault(other => Unit.IsWithinAttackRange(other)
                            && Unit.Faction.GetDiplomaticStance(other.Faction) == DiplomaticStance.Enemy);
                if (target != null)
                {
                    Unit.LookAt(target.Center);
                    Unit.TryHit(target);
                }
            }
        }

        private bool IsTargetValid(Unit target)
        {
            if (target != null)
                if (target.IsAlive)
                    if (Unit.IsWithinAttackRange(target))
                        return true;
            return false;
        }
        #endregion
    }
}
