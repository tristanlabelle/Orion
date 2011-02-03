using System;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to erect buildings.
    /// </summary>
    [Serializable]
    public sealed class HealSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat SpeedStat = new UnitStat(typeof(HealSkill), "Speed", "Vitesse de soin");
        public static readonly UnitStat RangeStat = new UnitStat(typeof(HealSkill), "Range", "Portée de soin");

        private int speed = 1;
        private int range = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the speed at which units are healed, in health points per second.
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
        /// Accesses the maximum distance between the healer and its target, in world units.
        /// </summary>
        public int Range
        {
            get { return range; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Range");
                range = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new HealSkill
            {
                speed = speed,
                range = range
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == SpeedStat) return speed;
            if (stat == RangeStat) return range;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == SpeedStat) Speed = value;
            else if (stat == RangeStat) Range = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
