using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Skills;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Health : Component
    {
        #region Static
        public static readonly EntityStat MaxHealthStat = new EntityStat(typeof(Health), "MaxHealth", "Points de vie maximum");
        public static readonly EntityStat RegenerationRateStat = new EntityStat(typeof(Health), "RegenerationRate", "Regénération");
        public static readonly EntityStat DamageReductionStat = new EntityStat(typeof(Health), "DamageReduction", "Réduction de dégâts");
        public static readonly EntityStat CurrentHealthStat = new EntityStat(typeof(Health), "CurrentHealth", "Points de vie");
        public static readonly EntityStat ArmorStat = new EntityStat(typeof(Health), "Armor", "Armure");
        #endregion

        #region Fields
        private int maxHealth;
        private float currentDamage;
        private float regenerationRate;
        private float damageReduction;
        private ArmorType armorType;
        private float armor;
        #endregion

        #region Constructors
        public Health(Entity entity, int maxHealth, ArmorType armorType, float armor, float regenerationRate, float damageReduction)
            : base(entity)
        {
            this.maxHealth = maxHealth;
            this.armor = armor;
            this.armorType = armorType;
            this.regenerationRate = regenerationRate;
            this.damageReduction = damageReduction;
        }
        #endregion

        #region Properties
        public int MaxHealth
        {
            get { return maxHealth; }
        }

        public ArmorType ArmorType
        {
            get { return armorType; }
        }

        public float Armor
        {
            get { return armor; }
        }

        public float CurrentHealth
        {
            get { return maxHealth - currentDamage; }
        }

        public float RegenerationRate
        {
            get { return regenerationRate; }
        }

        public float DamageReduction
        {
            get { return damageReduction; }
        }
        #endregion

        #region Methods
        public void Damage(float damage)
        {
            float actualDamage = damage - damageReduction;
            if (actualDamage < 0) actualDamage = 0;
            currentDamage += actualDamage;

            if (currentDamage >= Entity.GetStat(MaxHealthStat))
                Entity.Die();
        }

        public override void Update(SimulationStep step)
        {
            currentDamage -= regenerationRate * step.TimeDeltaInSeconds;
            if (currentDamage < 0) currentDamage = 0;
        }
        #endregion
    }
}
