using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

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
        #endregion

        #region Fields
        private float power;
        private float range;
        private float delay;
        private float splashRadius;
        private float timeElapsedSinceLastHit = float.PositiveInfinity;
        private readonly List<DamageFilter> damageFilters = new List<DamageFilter>();
        #endregion

        #region Constructors
        public Attack(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float Power
        {
            get { return power; }
            set { power = value; }
        }

        [Persistent]
        public float Range
        {
            get { return range; }
            set { range = value; }
        }

        [Mandatory]
        public float Delay
        {
            get { return delay; }
            set { delay = value; }
        }

        [Persistent]
        public float SplashRadius
        {
            get { return splashRadius; }
            set { splashRadius = value; }
        }

        [Persistent]
        public ICollection<DamageFilter> DamageFilters
        {
            get { return damageFilters; }
        }

        public float TimeElapsedSinceLastHit
        {
            get { return timeElapsedSinceLastHit; }
            set { timeElapsedSinceLastHit = value; }
        }
        #endregion

        #region Methods
        public float ApplyDamageFilters(float initalDamage, Entity target)
        {
            float finalDamage = initalDamage;
            foreach (DamageFilter filter in damageFilters)
            {
                if (filter.Applies(target))
                    finalDamage += filter.AdditionalDamage;
            }
            return finalDamage;
        }

        public override void Update(SimulationStep step)
        {
            timeElapsedSinceLastHit += step.TimeDeltaInSeconds;
        }
        #endregion
    }
}
