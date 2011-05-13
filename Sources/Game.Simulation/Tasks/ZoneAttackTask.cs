using System;
using OpenTK;
using Orion.Engine;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a<see cref="Entity"/> move to a location and attack enemies on it's way.
    /// </summary>
    [Serializable]
    public sealed class ZoneAttackTask : Task
    {
        #region Fields
        private readonly Vector2 destination;
        private AttackTask attack;
        private MoveTask move;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="ZoneAttack"/> task from the <see cref="Entity"/>
        /// that attacks and its destination.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> who attacks.</param>
        /// <param name="destination">The destination of the unit'</param>
        public ZoneAttackTask(Entity entity, Vector2 destination)
            : base(entity)
        {
            Argument.EnsureNotNull(entity, "entity");
            
            this.destination = destination;
            this.move = new MoveTask(entity, (Point)destination);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "attacking while moving to {0}".FormatInvariant(destination); }
        }

        public override Type PublicType
        {
            get { return attack == null ? typeof(MoveTask) : attack.PublicType; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            Attacker attacker = Entity.Components.TryGet<Attacker>();
            if (attacker == null)
            {
                MarkAsEnded();
                return;
            }

            if (attack == null && Entity.CanPerformHeavyOperation)
            {
                bool isRanged = attacker.IsRanged;

                Entity target = attacker.FindVisibleTarget();
                if (target != null) attack = new AttackTask(Entity, target);
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
