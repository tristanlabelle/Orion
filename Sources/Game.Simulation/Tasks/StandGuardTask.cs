using System;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
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
            : base(guard) {}
        #endregion

        #region Properties
        public override string Description
        {
            get { return "standing guard"; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Attacker attacker = Unit.Components.TryGet<Attacker>();
            if (attacker == null)
            {
                MarkAsEnded();
                return;
            }

            if (!IsTargetValid(target))
            {
                bool canAttemptFindingTarget = ((step.Number + Unit.Handle.Value) % enemyCheckInterval) == 0;
                if (canAttemptFindingTarget)
                {
                    target = Unit.World.Entities
                        .Intersecting(Unit.LineOfSight)
                        .OfType<Unit>()
                        .FirstOrDefault(other => Unit.IsWithinAttackRange(other)
                            && !Unit.Faction.GetDiplomaticStance(other.Faction).HasFlag(DiplomaticStance.AlliedVictory));
                }
                else
                {
                    target = null;
                }
            }

            if (target != null)
            {
                Unit.LookAt(target.Center);
                attacker.TryHit(target);
            }
        }

        private bool IsTargetValid(Unit target)
        {
            return target != null && target.IsAliveInWorld && Unit.IsWithinAttackRange(target);
        }
        #endregion
    }
}
