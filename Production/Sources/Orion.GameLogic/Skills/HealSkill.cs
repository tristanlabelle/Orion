using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to erect buildings.
    /// </summary>
    [Serializable]
    public sealed class HealSkill : Skill
    {
        #region Fields
        private int speed;
        #endregion

        #region Constructors
        
        public HealSkill(int speed)
        {
            Argument.EnsureStrictlyPositive(speed, "speed");
            this.speed = speed;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the building speed associated with this skill.
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        #endregion

        #region Methods
        
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.HealSpeed) return speed;
            return null;
        }
        #endregion
    }
}
