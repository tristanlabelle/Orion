using System;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes an <see cref="Entity"/> attack enemy units within range, without following.
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

        private Entity target;
        #endregion

        #region Constructors
        public StandGuardTask(Entity guard)
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
            Spatial spatial = Entity.Spatial;
            Attacker attacker = Entity.Components.TryGet<Attacker>();
            if (spatial == null || attacker == null)
            {
                MarkAsEnded();
                return;
            }

            if (!IsTargetValid(attacker, target))
            {
                bool canAttemptFindingTarget = ((step.Number + Entity.Handle.Value) % enemyCheckInterval) == 0;
                target = canAttemptFindingTarget ? attacker.FindVisibleTarget() : null;
            }

            if (target != null)
            {
                spatial.LookAt(target.Center);
                attacker.TryHit(target);
            }
        }

        private bool IsTargetValid(Attacker attacker, Entity target)
        {
            return target != null && target.IsAlive && attacker.IsInRange(target);
        }
        #endregion
    }
}
