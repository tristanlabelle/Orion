using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to attack.
    /// </summary>
    [Serializable]
    public sealed class Attack : Skill
    {
        #region Fields
        private readonly int power;
        private readonly int maxRange;
        #endregion

        #region Constructors
        public Attack(int power, int maxRange)
        {
            Argument.EnsureStrictlyPositive(power, "power");
            Argument.EnsurePositive(maxRange, "maxRange");

            this.power = power;
            this.maxRange = maxRange;
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
            return null;
        }
        #endregion
    }
}
