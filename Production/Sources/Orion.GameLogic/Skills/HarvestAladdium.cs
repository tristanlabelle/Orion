using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to collect aladdium from mines.
    /// </summary>
    [Serializable]
    [SkillDependency(typeof(Move))]
    public sealed class HarvestAladdium
    {
        #region Fields
        private readonly int maxCarryingAmount;
        #endregion

        #region Constructors
        public HarvestAladdium(int maxCarryingAmount)
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
