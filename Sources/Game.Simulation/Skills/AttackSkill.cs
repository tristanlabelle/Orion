using System;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="UnitType"/> to attack.
    /// </summary>
    [Serializable]
    public sealed class AttackSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat PowerStat = new UnitStat(typeof(AttackSkill), "Power", "Puissance d'attaque");
        public static readonly UnitStat RangeStat = new UnitStat(typeof(AttackSkill), "Range", "Portée d'attaque");
        public static readonly UnitStat DelayStat = new UnitStat(typeof(AttackSkill), "Delay", "Délai d'attaque");

        private int power = 1;
        private int range;
        private int delay = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the attack power, in health points.
        /// </summary>
        public int Power
        {
            get { return power; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Power");
                power = value;
            }
        }

        /// <summary>
        /// Accesses the maximum attack range, in world units.
        /// A value of one indicates a melee attacker.
        /// </summary>
        public int Range
        {
            get { return range; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "MaxRange");
                range = value;
            }
        }

        /// <summary>
        /// Accesses the amount of time between successive attacks, in seconds.
        /// </summary>
        public int Delay
        {
            get { return delay; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Delay");
                delay = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if this skill represents a melee attacker.
        /// </summary>
        public bool IsMelee
        {
            get { return range == 0; }
        }

        /// <summary>
        /// Gets a value indicating if this skill represents a ranged attacker.
        /// </summary>
        public bool IsRanged
        {
            get { return range > 0; }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new AttackSkill
            {
                power = power,
                range = range,
                delay = delay
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == PowerStat) return power;
            if (stat == RangeStat) return range;
            if (stat == DelayStat) return delay;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == PowerStat) Power = value;
            else if (stat == RangeStat) Range = value;
            else if (stat == DelayStat) Delay = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
