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
        private readonly int maxCarryingAmount;
        #endregion

        #region Constructors
        public Harvest(int maxCarryingAmount)
        {
            Argument.EnsureStrictlyPositive(maxCarryingAmount, "maxCarryingAmount");
            this.maxCarryingAmount = maxCarryingAmount;
        }
        #endregion

        #region Properties
        public int MaxCarryingAmount
        {
            get { return maxCarryingAmount; }
        }
        #endregion

        #region Methods
        #endregion
    }
}
