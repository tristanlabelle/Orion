using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine.Geometry;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to take part in attacking tasks.
    /// </summary>
    public sealed class Attacker : Component
    {
        #region Static
        #region Fields
        public static readonly Stat PowerStat = new Stat(typeof(Attacker), StatType.Real, "Power");
        public static readonly Stat RangeStat = new Stat(typeof(Attacker), StatType.Real, "Range");
        public static readonly Stat DelayStat = new Stat(typeof(Attacker), StatType.Real, "Delay");
        public static readonly Stat SplashRadiusStat = new Stat(typeof(Attacker), StatType.Real, "SplashRadius");
        #endregion
        #endregion

        #region Fields
        private float power = 1;
        private float range;
        private float delay = 1;
        private float splashRadius;
        private TimeSpan timeElapsedSinceLastHit = TimeSpan.MaxValue;
        private readonly List<DamageFilter> damageFilters = new List<DamageFilter>();
        #endregion

        #region Constructors
        public Attacker(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float Power
        {
            get { return power; }
            set
            {
                Argument.EnsurePositive(value, "Power");
                power = value;
            }
        }

        [Persistent]
        public float Range
        {
            get { return range; }
            set
            {
                Argument.EnsurePositive(value, "Range");
                range = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> attacks at a range.
        /// </summary>
        public bool IsMelee
        {
            get
            {
                // GetStatValue is not used because we do not want a technology increasing the
                // range and turning this entity from a melee to a ranged attacker.
                return range == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> attacks at a range.
        /// </summary>
        public bool IsRanged
        {
            get { return !IsMelee; }
        }

        [Mandatory]
        public float Delay
        {
            get { return delay; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Delay");
                delay = value;
            }
        }

        [Persistent]
        public float SplashRadius
        {
            get { return splashRadius; }
            set
            {
                Argument.EnsurePositive(value, "SplashRadius");
                splashRadius = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/>'s attack has splashing effects.
        /// </summary>
        public bool Splashes
        {
            get { return splashRadius > 0; }
        }

        [Persistent]
        public ICollection<DamageFilter> DamageFilters
        {
            get { return damageFilters; }
        }

        public TimeSpan TimeElapsedSinceLastHit
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

        /// <summary>
        /// Tests if a given <see cref="Entity"/> is within this <see cref="Entity"/>'s attack range.
        /// </summary>
        /// <param name="target">The target <see cref="Entity"/>.</param>
        /// <returns>A value indicating if <paramref name="target"/> is in the attack range.</returns>
        public bool IsInRange(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Spatial spatial = Entity.Spatial;
            Spatial targetSpatial = target.Spatial;
            if (spatial == null || targetSpatial == null) return false;

            if (IsMelee)
            {
                bool selfIsAirborne = spatial.CollisionLayer == CollisionLayer.Air;
                bool otherIsAirborne = targetSpatial.CollisionLayer == CollisionLayer.Air;
                if (!selfIsAirborne && otherIsAirborne) return false;

                return Region.AreAdjacentOrIntersecting(spatial.GridRegion, targetSpatial.GridRegion);
            }
            else
            {
                float range = (float)Entity.GetStatValue(Attacker.RangeStat);
                return spatial.IsInRange(target, range);
            }
        }

        /// <summary>
        /// Finds an <see cref="Entity"/> to attack within this <see cref="Entity"/>'s sight range.
        /// </summary>
        /// <returns>An <see cref="Entity"/> to attack, or <c>null</c> if no appropriate targets are visible.</returns>
        public Entity FindVisibleTarget()
        {
            Spatial spatial = Entity.Spatial;
            Vision vision = Entity.Components.TryGet<Vision>();
            if (spatial == null || vision == null) return null;

            Entity bestTarget = null;
            float bestScore = float.NegativeInfinity;
            foreach (Entity target in World.Entities.Intersecting(vision.LineOfSight))
            {
                Spatial targetSpatial = target.Spatial;
                if (target == Entity
                    || targetSpatial == null
                    || !vision.CanSee(target)
                    || !IsInRange(target)
                    || !target.Components.Has<Health>()
                    || FactionMembership.IsAlliedTo(Entity, target))
                {
                    continue;
                }

                float score = Region.Distance(spatial.GridRegion, targetSpatial.GridRegion);

                // Favor attackers first.
                if (target.Components.Has<Attacker>()) score += 100;

                if (score > bestScore) bestTarget = target;
            }

            return bestTarget;
        }

        public bool TryHit(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            TimeSpan hitDelayInSeconds = TimeSpan.FromSeconds((float)Entity.GetStatValue(Attacker.DelayStat));
            if (timeElapsedSinceLastHit < hitDelayInSeconds) return false;

            Hit(target);
            return true;
        }

        public void Hit(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth == null) return;

            float damage = GetDamage(targetHealth);
            targetHealth.Damage += damage;

            HitEventArgs args = new HitEventArgs(Entity, target);
            World.RaiseHitOccured(args);

            if (Splashes)
            {
                float splashRadius = (float)Entity.GetStatValue(Attacker.SplashRadiusStat);

                Faction faction = FactionMembership.GetFaction(Entity);

                Circle splashCircle = new Circle(target.Center, splashRadius);
                foreach (Entity splashedEntity in World.Entities.Intersecting(splashCircle))
                {
                    if (splashedEntity == target) continue;

                    Faction splashedEntityFaction = FactionMembership.GetFaction(splashedEntity);
                    bool isAllied = faction != null && splashedEntityFaction != null
                        && faction.GetDiplomaticStance(splashedEntityFaction).HasFlag(DiplomaticStance.AlliedVictory);
                    if (isAllied) continue;

                    Health splashedEntityHealth = splashedEntity.Components.TryGet<Health>();

                    float distance = (splashCircle.Center - splashedEntity.Spatial.Center).LengthFast;
                    if (distance > splashRadius) continue;

                    int splashedTargetArmor = (int)splashedEntity.GetStatValue(Health.ArmorStat);
                    splashedEntityHealth.Damage += GetDamage(splashedEntityHealth) * (1 - distance / splashRadius);
                }
            }

            timeElapsedSinceLastHit = TimeSpan.Zero;
        }

        public float GetDamage(Health targetHealth)
        {
            Argument.EnsureNotNull(targetHealth, "targetHealth");

            float damage = (float)Entity.GetStatValue(PowerStat);
            foreach (DamageFilter filter in damageFilters)
            {
                if (filter.Applies(targetHealth.Entity))
                    damage += filter.AdditionalDamage;
            }

            damage -= targetHealth.Armor;

            return Math.Max(0, damage);
        }

        public override void Update(SimulationStep step)
        {
            timeElapsedSinceLastHit += step.TimeDelta;
        }
        #endregion
    }
}
