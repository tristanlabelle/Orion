using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    public class Attack : Component
    {
        #region Static
        #region Fields
        public static readonly EntityStat PowerStat = new EntityStat(typeof(Attack), StatType.Real, "Power", "Puissance");
        public static readonly EntityStat RangeStat = new EntityStat(typeof(Attack), StatType.Real, "Range", "Portée");
        public static readonly EntityStat DelayStat = new EntityStat(typeof(Attack), StatType.Real, "Delay", "Délai");
        public static readonly EntityStat SplashRadiusStat = new EntityStat(typeof(Attack), StatType.Real, "SplashRadius", "Rayon de dégâts");
        #endregion

        #region Methods
        public static DamageFilter CreateArmorTypeDamageFilter(ArmorType armorType, float additionalDamage)
        {
            Argument.EnsureNotEqual(additionalDamage, 0, "additionalDamage");

            Func<Entity, float> filter = delegate(Entity e)
            {
                Health healthComponent = e.GetComponent<Health>();
                return healthComponent.ArmorType == armorType ? additionalDamage : 0;
            };

            char sign = additionalDamage > 0 ? '+' : '-';
            string description = "{0}{1} contre {2}".FormatInvariant(sign, additionalDamage, armorType);
            return new DamageFilter(filter, description);
        }
        #endregion
        #endregion

        #region Fields
        private float power;
        private float range;
        private float delay;
        private float splashRadius;
        private readonly List<DamageFilter> damageFilters;
        #endregion

        #region Constructors
        public Attack(Entity entity, float power, float range, float delay, float splashRadius)
            : base(entity)
        {
            this.power = power;
            this.range = range;
            this.delay = delay;
            this.splashRadius = splashRadius;
        }
        #endregion

        #region Properties
        public float Power
        {
            get { return power; }
        }

        public float Range
        {
            get { return range; }
        }

        public float Delay
        {
            get { return delay; }
        }

        public float SplashRadius
        {
            get { return splashRadius; }
        }
        #endregion

        #region Methods
        public void AddDamageFilter(DamageFilter filter)
        {
            damageFilters.Add(filter);
        }

        public void RemoveDamageFilter(DamageFilter filter)
        {
            damageFilters.Remove(filter);
        }

        public float ApplyDamageFilters(float initalDamage, Entity target)
        {
            float finalDamage = initalDamage;
            foreach (DamageFilter filter in damageFilters)
                finalDamage += filter.Apply(target);
            return finalDamage;
        }
        #endregion
    }
}
