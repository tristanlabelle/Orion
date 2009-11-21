using System;
using Orion.Geometry;


namespace Orion.GameLogic.Tasks
{    
    /// <summary>
    /// A <see cref="Task"/> which make attack an enemy <see cref="Unit"/>
    /// </summary>
    [Serializable]
    public sealed class Attack : Task
    {
        #region Fields
        private readonly Unit attacker;
        private readonly Unit target;
        private readonly Follow follow;
        private float hitDelayInSeconds;
        private float timeSinceLastHitInSeconds = 0;
        #endregion

        #region Constructors
        public Attack(Unit attacker, Unit target)
        {
            Argument.EnsureNotNull(attacker, "attacker");
            if (!attacker.HasSkill<Skills.Attack>())
                throw new ArgumentException("Cannot attack without the attack skill.", "attacker");
            Argument.EnsureNotNull(target, "target");
            
            this.attacker = attacker;
            this.target = target;
            if (attacker.HasSkill<Skills.Move>()) this.follow = new Follow(attacker, target);
            hitDelayInSeconds = attacker.GetStat(UnitStat.AttackDelay);
            timeSinceLastHitInSeconds = hitDelayInSeconds;
        }
        #endregion

        #region Properties
        public Unit Attacker
        {
            get { return attacker; }
        }

        public Unit Target
        {
            get { return target; }
        }

        public bool IsTargetInRange
        {
            get
            {
                float squaredDistanceToTarget = (target.Position - attacker.Position).LengthSquared;
                float attackRange = attacker.GetStat(UnitStat.AttackRange);
                return squaredDistanceToTarget <= attackRange * attackRange;
            }
        }

        public override string Description
        {
            get { return "attacking"; }
        }

        public override bool HasEnded
        {
            get
            {
                if (!attacker.HasSkill<Skills.Move>() && !IsTargetInRange)
                    return true;

                return !target.IsAlive;
            }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (HasEnded) return;

            if (IsTargetInRange)
            {
                timeSinceLastHitInSeconds += timeDelta;
                if (timeSinceLastHitInSeconds > hitDelayInSeconds)
                {
                    target.Damage += attacker.GetStat(UnitStat.AttackPower);
                    timeSinceLastHitInSeconds = 0;
                }
            }
            else if (follow != null)
            {
                follow.Update(timeDelta);
            }
        }
        #endregion
    }
}