using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// Basic skill which every <see cref="UnitType"/> has.
    /// </summary>
    [Serializable]
    public sealed class BasicSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat AlageneCostStat = new UnitStat(typeof(BasicSkill), "AlageneCost", "Coût en alagène");
        public static readonly UnitStat AladdiumCostStat = new UnitStat(typeof(BasicSkill), "AladdiumCost", "Coût en aladdium");
        public static readonly UnitStat FoodCostStat = new UnitStat(typeof(BasicSkill), "FoodCost", "Coût en nourriture");
        public static readonly UnitStat MaxHealthStat = new UnitStat(typeof(BasicSkill), "MaxHealth", "Points de vie maximum");
        public static readonly UnitStat MeleeArmorStat = new UnitStat(typeof(BasicSkill), "MeleeArmor", "Armure au corps-à-corps");
        public static readonly UnitStat RangedArmorStat = new UnitStat(typeof(BasicSkill), "RangedArmor", "Armure à distance");
        public static readonly UnitStat SightRangeStat = new UnitStat(typeof(BasicSkill), "SightRange", "Portée de vision");

        private int alageneCost;
        private int aladdiumCost;
        private int foodCost;
        private int maxHealth = 1;
        private int meleeArmor;
        private int rangedArmor;
        private int sightRange = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Gets cost of this unit type, in alagene points.
        /// </summary>
        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "AlageneCost");
                alageneCost = value;
            }
        }

        /// <summary>
        /// Gets cost of this unit type, in aladdium points.
        /// </summary>
        public int AladdiumCost
        {
            get { return aladdiumCost; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "AladdiumCost");
                aladdiumCost = value;
            }
        }

        /// <summary>
        /// Gets cost of this unit type, in food points.
        /// </summary>
        public int FoodCost
        {
            get { return foodCost; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "FoodCost");
                foodCost = value;
            }
        }

        /// <summary>
        /// Gets the maximum health points a unit can have.
        /// </summary>
        public int MaxHealth
        {
            get { return maxHealth; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "MaxHealth");
                maxHealth = value;
            }
        }

        /// <summary>
        /// Gets the amount of armor against melee attacks, in health points.
        /// </summary>
        public int MeleeArmor
        {
            get { return maxHealth; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "MeleeArmor");
                meleeArmor = value;
            }
        }

        /// <summary>
        /// Gets the amount of armor against ranged attacks, in health points.
        /// </summary>
        public int RangedArmor
        {
            get { return rangedArmor; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "RangedArmor");
                rangedArmor = value;
            }
        }

        /// <summary>
        /// Gets the radius of vision of units, in world units.
        /// </summary>
        public int SightRange
        {
            get { return sightRange; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "SightRange");
                sightRange = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new BasicSkill
            {
                aladdiumCost = aladdiumCost,
                alageneCost = alageneCost,
                foodCost = foodCost,
                maxHealth = maxHealth,
                meleeArmor = meleeArmor,
                rangedArmor = rangedArmor,
                sightRange = sightRange
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == AlageneCostStat) return alageneCost;
            if (stat == AladdiumCostStat) return aladdiumCost;
            if (stat == FoodCostStat) return foodCost;
            if (stat == MaxHealthStat) return maxHealth;
            if (stat == MeleeArmorStat) return meleeArmor;
            if (stat == RangedArmorStat) return rangedArmor;
            if (stat == SightRangeStat) return sightRange;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == AlageneCostStat) AlageneCost = value;
            else if (stat == AladdiumCostStat) AladdiumCost = value;
            else if (stat == FoodCostStat) FoodCost = value;
            else if (stat == MaxHealthStat) MaxHealth = value;
            else if (stat == MeleeArmorStat) MeleeArmor = value;
            else if (stat == RangedArmorStat) RangedArmor = value;
            else if (stat == SightRangeStat) SightRange = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
