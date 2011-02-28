using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Skills;
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
        public static readonly Stat MaximumValueStat = new Stat(typeof(Health), StatType.Integer, "MaximumValue");
        public static readonly Stat RegenerationRateStat = new Stat(typeof(Health), StatType.Real, "RegenerationRate");
        public static readonly Stat DamageReductionStat = new Stat(typeof(Health), StatType.Integer, "DamageReduction");
        public static readonly Stat ValueStat = new Stat(typeof(Health), StatType.Integer, "Value");
        public static readonly Stat ArmorStat = new Stat(typeof(Health), StatType.Real, "Armor");
        #endregion

        #region Fields
        private int maximumValue = 1;
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

        #region Events
        /// <summary>
        /// Raised when the host <see cref="Entity"/> gets damaged or healed.
        /// </summary>
        public event Action<Health> DamageChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the maximum number of health points that can be had by the host <see cref="Entity"/>.
        /// </summary>
        [Mandatory]
        public int MaximumValue
        {
            get { return maximumValue; }
            set { maximumValue = value; }
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

        /// <summary>
        /// Accesses the current health value of the host <see cref="Entity"/>.
        /// </summary>
        [Transient]
        public float Value
        {
            get { return maximumValue - damage; }
            set { Damage = maximumValue - value; }
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
            set
            {
                Argument.EnsureNotNaN(value, "value");
                float actualDamage = value - damageReduction;

                if (value < 0) value = 0;
                if (value > MaximumValue) value = MaximumValue;

                damage = value;
                DamageChanged.Raise(this);

                if (damage >= Entity.GetStatValue(MaximumValueStat).IntegerValue)
                    Entity.Die();
            }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="Entity"/> can take part
        /// in life relinquishing activities.
        /// </summary>
        public bool CanSuicide
        {
            get { return canSuicide; }
            set { canSuicide = value; }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            float regeneration = regenerationRate * step.TimeDeltaInSeconds;
            float newDamage = damage - regeneration;
            if (newDamage < 0) newDamage = 0;
            if (newDamage != damage) DamageChanged.Raise(this);
            damage = newDamage;
        }
        #endregion
    }
}
