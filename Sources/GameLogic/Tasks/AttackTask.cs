using System;
using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic.Tasks
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
            if (!attacker.HasSkill<Skills.AttackSkill>())
                throw new ArgumentException("Cannot attack without the attack skill.", "attacker");
            Argument.EnsureNotNull(target, "target");
            
            this.target = target;
            if (attacker.HasSkill<Skills.MoveSkill>()) this.follow = new FollowTask(attacker, target);
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

        public override bool HasEnded
        {
            get
            {
                if (!Unit.Faction.CanSee(target))
                    return true;
                if (!Unit.IsWithinAttackRange(target))
                    return !Unit.HasSkill<Skills.MoveSkill>() || follow.HasEnded;
                return !target.IsAlive;
            }
        }
        #endregion

        #region Methods
        protected override void DoUpdate(SimulationStep step)
        {
            if (Unit.IsWithinAttackRange(target))
            {
                Unit.LookAt(target.Center);
                Unit.TryHit(target);
            }
            else if (follow != null)
            {
                follow.Update(step);
            }
        }
        #endregion
    }
}