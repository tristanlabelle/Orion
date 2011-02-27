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
    /// A <see cref="Task"/> which makes a<see cref="Unit"/> move to a location and attack enemies on it's way.
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
            this.move = new MoveTask(unit, (Point)destination);
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "attacking while moving to {0}".FormatInvariant(destination); }
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

            if (attack == null && ((Unit)Entity).CanPerformProximityChecks(step))
            {
                bool isRanged = attacker.IsRanged;

                Entity target = attacker.FindVisibleTarget();
                if (target != null) attack = new AttackTask(target, target);
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
