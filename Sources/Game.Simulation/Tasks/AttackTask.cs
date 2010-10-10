using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Tasks
{    
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    [Serializable]
    public sealed class AttackTask : Task
    {
        #region Fields
        private readonly Unit target;
        private readonly FollowTask follow;
        #endregion

        #region Constructors
        public AttackTask(Unit attacker, Unit target)
            : base(attacker)
        {
            Argument.EnsureNotNull(attacker, "attacker");
            if (!attacker.HasSkill<AttackSkill>())
                throw new ArgumentException("Cannot attack without the attack skill.", "attacker");
            Argument.EnsureNotNull(target, "target");
            
            this.target = target;
            if (attacker.HasSkill<MoveSkill>()) this.follow = new FollowTask(attacker, target);
        }
        #endregion

        #region Properties
        public Unit Attacker
        {
            get { return Unit; }
        }

        public Unit Target
        {
            get { return target; }
        }

        public override string Description
        {
            get { return "attacking"; }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (!target.IsAliveInWorld || !Unit.Faction.CanSee(target))
            {
                // If the target has died while we weren't yet in attack range,
                // or if the unit moved out of sight,  but we're coming, complete the motion with a move task.
                if (follow != null && !Unit.IsWithinAttackRange(target) && Unit.TaskQueue.Count == 1)
                    Unit.TaskQueue.OverrideWith(new MoveTask(Unit, (Point)target.Center));
                MarkAsEnded();
                return;
            }

            if (Unit.IsWithinAttackRange(target))
            {
                Unit.LookAt(target.Center);
                Unit.TryHit(target);
                return;
            }
            else
            {
                if (follow == null || follow.HasEnded)
                {
                    MarkAsEnded();
                    return;
                }

                follow.Update(step);
            }
        }
        #endregion
    }
}