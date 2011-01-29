using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Skills;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Health : Component
    {
        #region Static
        public static readonly EntityStat MaxHealthStat = new EntityStat(typeof(Health), StatType.Integer, "MaxHealth", "Points de vie maximum");
        public static readonly EntityStat RegenerationRateStat = new EntityStat(typeof(Health), StatType.Real, "RegenerationRate", "Regénération");
        public static readonly EntityStat DamageReductionStat = new EntityStat(typeof(Health), StatType.Integer, "DamageReduction", "Réduction de dégâts");
        public static readonly EntityStat CurrentHealthStat = new EntityStat(typeof(Health), StatType.Integer, "CurrentHealth", "Points de vie");
        public static readonly EntityStat ArmorStat = new EntityStat(typeof(Health), StatType.Real, "Armor", "Armure");
        #endregion

        #region Fields
        private int maxHealth;
        private ArmorType armorType;
        private int threatLevel;
        private float regenerationRate;
        private float damageReduction;
        private float armor;
        private float currentDamage;
        #endregion

        #region Constructors
        public Health(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        [Mandatory]
        public ArmorType ArmorType
        {
            get { return armorType; }
            set { armorType = value; }
        }

        [Mandatory]
        public float Armor
        {
            get { return armor; }
            set { armor = value; }
        }

        [Mandatory]
        public int ThreatLevel
        {
            get { return threatLevel; }
            set { threatLevel = value; }
        }

        [Transient]
        public float CurrentHealth
        {
            get { return maxHealth - currentDamage; }
            set { currentDamage = maxHealth - value; }
        }

        [Persistent]
        public float RegenerationRate
        {
            get { return regenerationRate; }
            set { regenerationRate = value; }
        }

        [Persistent]
        public float DamageReduction
        {
            get { return damageReduction; }
            set { damageReduction = value; }
        }
        #endregion

        #region Methods
        public void Damage(float damage)
        {
            float actualDamage = damage - damageReduction;
            if (actualDamage < 0) actualDamage = 0;
            currentDamage += actualDamage;

            if (currentDamage >= Entity.GetStat(MaxHealthStat).IntegerValue)
                Entity.Die();
        }
        #endregion
    }
}
