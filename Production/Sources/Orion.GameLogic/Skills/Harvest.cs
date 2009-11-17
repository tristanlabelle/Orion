using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to collect resources.
    /// </summary>
    [Serializable]
    [SkillDependency(typeof(Move))]
    public sealed class Harvest : Skill
    {
        #region Fields
        private readonly int speed;
        private readonly int maxCarryingAmount;
        #endregion

        #region Constructors
        public Harvest(int speed, int maxCarryingAmount)
        {
            Argument.EnsureStrictlyPositive(speed, "speed");
            Argument.EnsureStrictlyPositive(maxCarryingAmount, "maxCarryingAmount");
            this.speed = speed;
            this.maxCarryingAmount = maxCarryingAmount;
        }
        #endregion

        #region Properties
        public int Speed
        {
            get { return speed; }
        }

        public int MaxCarryingAmount
        {
            get { return maxCarryingAmount; }
        }
        #endregion

        #region Methods
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.ExtractingSpeed) return speed;
            if (stat == UnitStat.MaxCarryingAmount) return maxCarryingAmount;
            return null;
        }
        #endregion
    }
}
