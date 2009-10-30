using System;


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
        private const float hitDelayInSeconds = 1;
        private float timeSinceLastHitInSeconds = 0;
        private Follow follow;
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
            this.follow = new Follow(attacker, target, attacker.GetStat(UnitStat.AttackRange));
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

        public override string Description
        {
            get { return "attacking"; }
        }

        public override bool HasEnded
        {
            get { return !target.IsAlive; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// At each update it check if the striker is near enough to strike if not he reupdate the follow.update method.
        /// </summary>
        /// <param name="timeDelta"></param>
        public override void Update(float timeDelta)
        {
            if (HasEnded)
                return;

            if (follow.IsInRange)
            {
                if (TryInflictDamage(timeDelta))
                    target.Damage += attacker.GetStat(UnitStat.AttackPower);
            }
            else
            {
                follow.Update(timeDelta);

            }

        }
        
        /// <summary>
        /// Calculates the number of time elapsed in seconds and 
        /// inflicts damage to the enemy; dependant of the constant 
        /// named "secondsToHitEnemy". 
        /// </summary>
        private bool TryInflictDamage(float timeDelta)
        {
            if (timeSinceLastHitInSeconds >= hitDelayInSeconds)
            {
                timeSinceLastHitInSeconds = 0;
                return true;
            }
            else
            {
                timeSinceLastHitInSeconds += timeDelta;
                return false;
            }
        }
        #endregion
    }
}