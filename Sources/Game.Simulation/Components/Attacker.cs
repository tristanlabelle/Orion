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
        private float timeElapsedSinceLastHit = float.PositiveInfinity;
        private readonly ICollection<ArmorType> superEffectiveTargets = new HashSet<ArmorType>();
        private readonly ICollection<ArmorType> ineffectiveTargets = new HashSet<ArmorType>();
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
        public ICollection<ArmorType> SuperEffectiveTargets
        {
            get { return superEffectiveTargets; }
        }

        [Persistent]
        public ICollection<ArmorType> IneffectiveTargets
        {
            get { return ineffectiveTargets; }
        }

        public float TimeElapsedSinceLastHit
        {
            get { return timeElapsedSinceLastHit; }
            set { timeElapsedSinceLastHit = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds an <see cref="Entity"/> to attack within this <see cref="Entity"/>'s sight range.
        /// </summary>
        /// <returns>An <see cref="Entity"/> to attack, or <c>null</c> if no enemies are visible.</returns>
        public Entity FindVisibleTarget()
        {
            Vision vision = Entity.Components.TryGet<Vision>();
            if (Entity.Spatial == null || vision == null) return null;

            foreach (Entity target in World.Entities.Intersecting(vision.LineOfSight))
            {
                Spatial targetSpatial = target.Spatial;
                if (targetSpatial != null
                    && IsInRange(target)
                    && (IsRanged || targetSpatial.CollisionLayer == CollisionLayer.Ground)
                    && !FactionMembership.IsAlliedTo(Entity, target))
                {
                    return target;
                }
            }
            return null;
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

        public bool TryHit(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            float hitDelayInSeconds = (float)Entity.GetStatValue(Attacker.DelayStat);
            if (timeElapsedSinceLastHit < hitDelayInSeconds)
                return false;

            Hit(target);
            return true;
        }

        public void Hit(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth == null) return;

            float damage = GetDamage(targetHealth);
            targetHealth.Value -= damage;
            ((Unit)Entity).OnHitting((Unit)target, damage);

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
                    splashedEntityHealth.Value -= GetDamage(splashedEntityHealth) * (1 - distance / splashRadius);
                }
            }

            timeElapsedSinceLastHit = 0;
        }

        public float GetDamage(Health targetHealth)
        {
            Argument.EnsureNotNull(targetHealth, "targetHealth");

            float damage = (float)Entity.GetStatValue(PowerStat);
            
            if (superEffectiveTargets.Contains(targetHealth.ArmorType)) damage *= 2;
            if (ineffectiveTargets.Contains(targetHealth.ArmorType)) damage /= 2;

            damage -= targetHealth.Armor;

            return Math.Max(0, damage);
        }

        public override void Update(SimulationStep step)
        {
            timeElapsedSinceLastHit += step.TimeDeltaInSeconds;
        }
        #endregion
    }
}
