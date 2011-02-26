using System;
using Orion.Engine;
using System.Collections.Generic;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to attack.
    /// </summary>
    [Serializable]
    public sealed class AttackSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat PowerStat = new UnitStat(typeof(AttackSkill), "Power");
        public static readonly UnitStat RangeStat = new UnitStat(typeof(AttackSkill), "Range");
        public static readonly UnitStat DelayStat = new UnitStat(typeof(AttackSkill), "Delay");
        public static readonly UnitStat SplashRadiusStat = new UnitStat(typeof(AttackSkill), "SplashRadius");
        public static readonly UnitStat SuperEffectiveAgainstStat = new UnitStat(typeof(AttackSkill), "EffectiveAgainst");
        public static readonly UnitStat InffectiveAgainstStat = new UnitStat(typeof(AttackSkill), "IneffectiveAgainst");

        private int power = 1;
        private int range;
        private int delay = 1;
        private int splashRadius;
        private List<ArmorType> superEffectiveAgainst = new List<ArmorType>();
        private List<ArmorType> ineffectiveAgainst = new List<ArmorType>();
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
        /// Returns the list of armor types this attack skill is very effective against
        /// </summary>
        public List<ArmorType> SuperEffectiveAgainst
        {
            get { return superEffectiveAgainst; }
            set
            {
                EnsureNotFrozen();
                superEffectiveAgainst = value;
            }
        }

        /// <summary>
        /// Returns the list of armor types this attack skill is ineffective against
        /// </summary>
        public List<ArmorType> IneffectiveAgainst
        {
            get { return ineffectiveAgainst; }
            set
            {
                EnsureNotFrozen();
                ineffectiveAgainst = value;
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
        /// Accesses the radius in which other enemy units are damaged after an attack.
        /// </summary>
        public int SplashRadius
        {
            get { return splashRadius; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "SplashRadius");
                splashRadius = value;
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

        /// <summary>
        /// Gets a value indicating if this skill represents an attacker that does splash damage.
        /// </summary>
        public bool HasSplashDamage
        {
            get { return splashRadius > 0; }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new AttackSkill
            {
                power = power,
                range = range,
                delay = delay,
                splashRadius = splashRadius,
                superEffectiveAgainst = superEffectiveAgainst,
                ineffectiveAgainst = ineffectiveAgainst
            };
        }

        /// <summary>
        /// Tests if this <see cref="Unit"/> is super effective against the specified unit type
        /// </summary>
        /// <param name="type">The type of armor to check against</param>
        /// <returns>True if this unit is super effective against the specified type, else if not.</returns>
        public bool IsSuperEffectiveAgainst(ArmorType armorType)
        {
            return SuperEffectiveAgainst.Contains(armorType);
        }

        /// <summary>
        /// Tests if this <see cref="Unit"/> is ineffective against the specified unit type
        /// </summary>
        /// <param name="type">The type of armor to check against</param>
        /// <returns>True if this unit is ineffective against the specified type, else if not.</returns>
        public bool IsIneffectiveAgainst(ArmorType armorType)
        {
            return IneffectiveAgainst.Contains(armorType);
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == PowerStat) return power;
            if (stat == RangeStat) return range;
            if (stat == DelayStat) return delay;
            if (stat == SplashRadiusStat) return splashRadius;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == PowerStat) Power = value;
            else if (stat == RangeStat) Range = value;
            else if (stat == DelayStat) Delay = value;
            else if (stat == SplashRadiusStat) SplashRadius = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
