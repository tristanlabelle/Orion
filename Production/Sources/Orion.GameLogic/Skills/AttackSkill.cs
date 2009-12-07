using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to attack.
    /// </summary>
    [Serializable]
    public sealed class AttackSkill : Skill
    {
        #region Fields
        private readonly int power;
        private readonly int maxRange;
        private readonly int delay;
        #endregion

        #region Constructors
        public AttackSkill(int power, int maxRange, int delay)
        {
            Argument.EnsureStrictlyPositive(power, "power");
            Argument.EnsurePositive(maxRange, "maxRange");
            Argument.EnsureStrictlyPositive(delay, "delay");

            this.power = power;
            this.maxRange = maxRange;
            this.delay = delay;
        }
        #endregion

        #region Properties
        public int Power
        {
            get { return power; }
        }

        public int MaxRange
        {
            get { return maxRange; }
        }

        public int Delay
        {
            get { return delay; }
        }

        public bool IsMelee
        {
            get { return maxRange == 0; }
        }

        public bool IsRanged
        {
            get { return maxRange > 0; }
        }
        #endregion

        #region Methods
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.AttackPower) return power;
            if (stat == UnitStat.AttackRange) return maxRange;
            if (stat == UnitStat.AttackDelay) return delay;
            return null;
        }
        #endregion
    }
}
