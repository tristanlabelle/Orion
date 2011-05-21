using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Provides an <see cref="Entity"/> with a health value,
    /// which makes it damageable by damaging actions.
    /// </summary>
    public sealed class Health : Component
    {
        #region Static
        public static readonly Stat MaxValueStat = new Stat(typeof(Health), StatType.Integer, "MaxValue");
        public static readonly Stat RegenerationRateStat = new Stat(typeof(Health), StatType.Real, "RegenerationRate");
        public static readonly Stat DamageReductionStat = new Stat(typeof(Health), StatType.Integer, "DamageReduction");
        public static readonly Stat ValueStat = new Stat(typeof(Health), StatType.Integer, "Value");
        public static readonly Stat ArmorStat = new Stat(typeof(Health), StatType.Real, "Armor");
        #endregion

        #region Fields
        private int maxValue = 1;
        private Constitution constitution;
        private ArmorType armorType;
        private int threatLevel;
        private float regenerationRate;
        private float damageReduction;
        private float armor;
        private float damage;
        private bool canSuicide = true;
        #endregion

        #region Constructors
        public Health(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the maximum number of health points that can be had by the host <see cref="Entity"/>.
        /// </summary>
        [Persistent(true)]
        public int MaxValue
        {
            get { return maxValue; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "MaxValue");
                maxValue = value;
            }
        }

        /// <summary>
        /// Accesses the <see cref="Constitution"/> of the <see cref="Entity"/>,
        /// determining if it supports being repaired or healed.
        /// </summary>
        [Persistent]
        public Constitution Constitution
        {
            get { return constitution; }
            set
            {
                Argument.EnsureDefined(value, "Constitution");
                this.constitution = value;
            }
        }

        [Persistent(true)]
        public ArmorType ArmorType
        {
            get { return armorType; }
            set { armorType = value; }
        }

        [Persistent(true)]
        public float Armor
        {
            get { return armor; }
            set { armor = value; }
        }

        [Persistent(true)]
        public int ThreatLevel
        {
            get { return threatLevel; }
            set { threatLevel = value; }
        }

        /// <summary>
        /// Accesses the current health value of the host <see cref="Entity"/>.
        /// </summary>
        public float Value
        {
            get { return (float)Entity.GetStatValue(MaxValueStat) - damage; }
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

        /// <summary>
        /// Accesses the current amount of health lost by the host <see cref="Entity"/>.
        /// </summary>
        public float Damage
        {
            get { return damage; }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="Entity"/> can take part
        /// in user-triggered life relinquishing activities.
        /// </summary>
        public bool CanSuicide
        {
            get { return canSuicide; }
            set { canSuicide = value; }
        }
        #endregion

        #region Methods
        public void SetValue(float value)
        {
            Argument.EnsurePositive(value, "value");
            int maxValue = (int)Entity.GetStatValue(MaxValueStat);
            if (value > maxValue) value = maxValue;
            damage = maxValue - value;
        }

        /// <summary>
        /// Decreases the health value of this entity.
        /// </summary>
        /// <param name="amount">The amount of health points to lose.</param>
        /// <returns>
        /// A value indicating if the entity has been killed.
        /// </returns>
        public bool Hurt(float amount)
        {
            Argument.EnsurePositive(amount, "amount");
            if (amount == 0) return false;

            damage += amount;

            return DieIfOutOfHealth();
        }

        /// <summary>
        /// Increses the health value of this entity.
        /// </summary>
        /// <param name="amount">The amount of health points to gain.</param>
        public void Heal(float amount)
        {
            Argument.EnsurePositive(amount, "amount");
            if (amount == 0) return;

            damage = Math.Max(0, damage - amount);
        }

        public void FullyHeal()
        {
            damage = 0;
        }

        /// <summary>
        /// Kills this <see cref="Entity"/>.
        /// This method works even if <see cref="CanSuicide"/> is <c>false</c>.
        /// </summary>
        public void Suicide()
        {
            damage = (int)Entity.GetStatValue(MaxValueStat);
            Entity.Die();
        }

        protected override void Update(SimulationStep step)
        {
            if (DieIfOutOfHealth()) return;

            Heal(regenerationRate * step.TimeDeltaInSeconds);
        }

        private bool DieIfOutOfHealth()
        {
            if (damage < (int)Entity.GetStatValue(MaxValueStat)) return false;
            
            Entity.Die();
            return true;
        }
        #endregion
    }
}
