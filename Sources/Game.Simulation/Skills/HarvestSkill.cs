using System;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to collect resources.
    /// </summary>
    [Serializable]
    [SkillDependency(typeof(MoveSkill))]
    public sealed class HarvestSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat SpeedStat = new UnitStat(typeof(HarvestSkill), "Speed");
        public static readonly UnitStat MaxCarryingAmountStat = new UnitStat(typeof(HarvestSkill), "MaxCarryingAmount");

        private int speed = 1;
        private int maxCarryingAmount = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the speed at which a unit collects resources, in points per second.
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Speed");
                speed = value;
            }
        }

        /// <summary>
        /// Accesses the maximum amount of resources a unit can carry at once
        /// between the resource node and the depot.
        /// </summary>
        public int MaxCarryingAmount
        {
            get { return maxCarryingAmount; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "MaxCarryingAmount");
                maxCarryingAmount = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new HarvestSkill
            {
                speed = speed,
                maxCarryingAmount = maxCarryingAmount
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == SpeedStat) return speed;
            if (stat == MaxCarryingAmountStat) return maxCarryingAmount;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == SpeedStat) Speed = value;
            else if (stat == MaxCarryingAmountStat) MaxCarryingAmount = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
