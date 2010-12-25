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
        #region enum
        public enum ArmorTypes
        {
            Light = 1,
            Heavy = 2,
            Surnatural = 3,
            Divine = 4
        };
        #endregion

        #region Fields
        public static readonly UnitStat AlageneCostStat = new UnitStat(typeof(BasicSkill), "AlageneCost", "Coût en alagène");
        public static readonly UnitStat AladdiumCostStat = new UnitStat(typeof(BasicSkill), "AladdiumCost", "Coût en aladdium");
        public static readonly UnitStat FoodCostStat = new UnitStat(typeof(BasicSkill), "FoodCost", "Coût en nourriture");
        public static readonly UnitStat MaxHealthStat = new UnitStat(typeof(BasicSkill), "MaxHealth", "Points de vie maximum");
        public static readonly UnitStat ArmorTypeStat = new UnitStat(typeof(BasicSkill), "ArmorType", "Type d'armure");
        public static readonly UnitStat ArmorStat = new UnitStat(typeof(BasicSkill), "Armor", "Armure");
        public static readonly UnitStat SightRangeStat = new UnitStat(typeof(BasicSkill), "SightRange", "Portée de vision");

        private int alageneCost;
        private int aladdiumCost;
        private int foodCost;
        private int maxHealth = 1;
        private ArmorTypes armorType;
        private int armor;
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
        /// Gets the units type of armor.
        /// </summary>
        public ArmorTypes ArmorType
        {
            get { return armorType; }
            set
            {
                EnsureNotFrozen();
                armorType = value;
            }
        }

        /// <summary>
        /// Gets the amount of armor against attacks, in health points.
        /// </summary>
        public int Armor
        {
            get { return armor; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsurePositive(value, "armor");
                armor = value;
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
                armor = armor,
                sightRange = sightRange,
                armorType = armorType
            };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == AlageneCostStat) return alageneCost;
            if (stat == AladdiumCostStat) return aladdiumCost;
            if (stat == FoodCostStat) return foodCost;
            if (stat == MaxHealthStat) return maxHealth;
            if (stat == ArmorTypeStat) return (int)armorType;
            if (stat == ArmorStat) return armor;
            if (stat == SightRangeStat) return sightRange;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == AlageneCostStat) AlageneCost = value;
            else if (stat == AladdiumCostStat) AladdiumCost = value;
            else if (stat == FoodCostStat) FoodCost = value;
            else if (stat == MaxHealthStat) MaxHealth = value;
            else if (stat == ArmorTypeStat) ArmorType = (ArmorTypes)value;
            else if (stat == ArmorStat) Armor = value;
            else if (stat == SightRangeStat) SightRange = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
