using System;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic.Skills;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes an <see cref="Unit"/> attack enemy units within range, without following.
    /// </summary>
    [Serializable]
    public sealed class StandGuardTask : Task
    {
        #region Fields
        /// <summary>
        /// The number of frames between checks for surrounding enemies.
        /// Used to distribute enemy checks on multiple frames so its not too heavy.
        /// </summary>
        private static int enemyCheckInterval = 6;

        private Unit target;
        #endregion

        #region Constructors
        public StandGuardTask(Unit guard)
            : base(guard)
        {}
        #endregion

        #region Properties
        public Unit Target
        {
            get { return target; }
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
            if (!Unit.HasSkill<AttackSkill>()) return;

            if (!IsTargetValid(target))
            {
                bool canAttemptFindingTarget = ((step.Number + Unit.Handle.Value) % enemyCheckInterval) == 0;
                if (canAttemptFindingTarget)
                {
                    target = Unit.World.Entities
                        .Intersecting(Unit.LineOfSight)
                        .OfType<Unit>()
                        .FirstOrDefault(other => Unit.IsWithinAttackRange(other)
                            && Unit.Faction.GetDiplomaticStance(other.Faction) == DiplomaticStance.Enemy);
                }
                else
                {
                    target = null;
                }
            }

            if (target != null)
            {
                Unit.LookAt(target.Center);
                Unit.TryHit(target);
            }
        }

        private bool IsTargetValid(Unit target)
        {
            return target != null && target.IsAlive && Unit.IsWithinAttackRange(target);
        }
        #endregion
    }
}
