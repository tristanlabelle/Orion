using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Identifies a stat associated with a unit.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class UnitStat
    {
        #region Instance
        #region Fields
        private readonly string name;
        private readonly UnitSkill? skill;
        private readonly int? defaultValue;
        #endregion

        #region Constructors
        private UnitStat(string name, UnitSkill skill)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureDefined(skill, "skill");
            
            this.name = name;
            this.skill = skill;
        }

        private UnitStat(string name, int defaultValue)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsurePositive(defaultValue, "defaultValue");

            this.name = name;
            this.defaultValue = defaultValue;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        public bool HasAssociatedSkill
        {
            get { return skill.HasValue; }
        }

        public UnitSkill AssociatedSkill
        {
            get { return skill.Value; }
        }

        public bool HasDefaultValue
        {
            get { return defaultValue.HasValue; }
        }

        public int DefaultValue
        {
            get { return defaultValue.Value; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
 	         return name;
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        #region Skill-Agnostic
        /// <summary>
        /// Specifies the cost of creation of a unit, in alagene points.
        /// </summary>
        public static readonly UnitStat AlageneCost = new UnitStat("AlageneCost", 0);

        /// <summary>
        /// Specifies the cost of creation of a unit, in aladdium points.
        /// </summary>
        public static readonly UnitStat AladdiumCost = new UnitStat("AladdiumCost", 0);

        /// <summary>
        /// Specifies the amount of food a unit takes, in food points.
        /// </summary>
        public static readonly UnitStat FoodCost = new UnitStat("FoodCost", 0);

        /// <summary>
        /// Specifies the maximum amount of health of a unit, in health points.
        /// </summary>
        public static readonly UnitStat MaxHealth = new UnitStat("MaxHealth", 1);

        /// <summary>
        /// Specifies the amount of damage subtracted from melee hits for a unit, in health points.
        /// </summary>
        public static readonly UnitStat MeleeArmor = new UnitStat("MeleeArmor", 0);

        /// <summary>
        /// Specifies the amount of damage subtracted from ranged hits for a unit, in health points.
        /// </summary>
        public static readonly UnitStat RangedArmor = new UnitStat("RangedArmor", 0);

        /// <summary>
        /// Specifies the radius of the fog-of-war circle clearing of a unit, in world units.
        /// </summary>
        public static readonly UnitStat SightRange = new UnitStat("SightRange", 1);
        #endregion

        #region Skill-Specific
        /// <summary>
        /// Specifies the maximum distance between a unit and its attack target, in world units.
        /// A value of zero indicates that the unit is a melee fighter.
        /// </summary>
        public static readonly UnitStat AttackRange = new UnitStat("AttackRange", UnitSkill.Attack);

        /// <summary>
        /// Specifies the amount of damage a unit does with each hit, in health points.
        /// </summary>
        public static readonly UnitStat AttackPower = new UnitStat("AttackPower", UnitSkill.Attack);

        /// <summary>
        /// Specifies the delay between successive hits, in seconds.
        /// </summary>
        public static readonly UnitStat AttackDelay = new UnitStat("AttackDelay", UnitSkill.Attack);

        /// <summary>
        /// Specifies the speed at which a unit builds, in health points per second.
        /// </summary>
        public static readonly UnitStat BuildSpeed = new UnitStat("BuildSpeed", UnitSkill.Build);

        /// <summary>
        /// Specifies the radius of the explosion of a unit, in world units.
        /// </summary>
        public static readonly UnitStat SuicideBombRadius = new UnitStat("SuicideBombRadius", UnitSkill.SuicideBomb);

        /// <summary>
        /// Specifies the damage caused by the explosion of a unit, in health points.
        /// </summary>
        public static readonly UnitStat SuicideBombDamage = new UnitStat("SuicideBombDamage", UnitSkill.SuicideBomb);

        /// <summary>
        /// Specifies the speed at which a unit harvest, in resource points per second.
        /// </summary>
        public static readonly UnitStat HarvestSpeed = new UnitStat("HarvestSpeed", UnitSkill.Harvest);

        /// <summary>
        /// Specifies the maximum amount of resources a unit may carry at once, in resource points.
        /// </summary>
        public static readonly UnitStat MaxCarryingAmount = new UnitStat("MaxCarryingAmount", UnitSkill.Harvest);

        /// <summary>
        /// Specifies the speed at which a unit moves, in world units per second.
        /// </summary>
        public static readonly UnitStat MoveSpeed = new UnitStat("MoveSpeed", UnitSkill.Move);

        /// <summary>
        /// Specifies the speed at which a building trains units, in health points per second.
        /// </summary>
        public static readonly UnitStat TrainSpeed = new UnitStat("TrainSpeed", UnitSkill.Train);

        /// <summary>
        /// Specifies the amount of food provided by a building, in food points.
        /// </summary>
        public static readonly UnitStat StoreFoodCapacity = new UnitStat("StoreFoodCapacity", UnitSkill.StoreFood);

        /// <summary>
        /// Specifies the speed at which a unit heals other units, in health points per second.
        /// </summary>
        public static readonly UnitStat HealSpeed = new UnitStat("HealSpeed", UnitSkill.Heal);

        /// <summary>
        /// Specifies the maximum distance between a unit and its heal target, in world units.
        /// </summary>
        public static readonly UnitStat HealRange = new UnitStat("HealRange", UnitSkill.Heal);
        #endregion

        private static readonly ReadOnlyCollection<UnitStat> values;
        #endregion

        #region Constructor
        static UnitStat()
        {
            values = typeof(UnitStat).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(property => {
                    UnitStat stat = (UnitStat)property.GetValue(null);
                    Debug.Assert(stat.Name == property.Name);
                    return stat;
                })
                .ToList()
                .AsReadOnly();
        }
        #endregion

        #region Properties
        public static ReadOnlyCollection<UnitStat> Values
        {
            get { return values; }
        }
        #endregion

        #region Methods
        public static UnitStat Parse(string str)
        {
            Argument.EnsureNotNull(str, "str");

            UnitStat stat = Values.FirstOrDefault(s => s.Name == str);
            if (stat == null) throw new ArgumentException("Invalid UnitStat name: {0}.".FormatInvariant(str));

            return stat;
        }
        #endregion
        #endregion
    }
}
