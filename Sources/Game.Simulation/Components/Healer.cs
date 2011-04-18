using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A <see cref="Component"/> which allows an <see cref="Entity"/>
    /// to execute healing tasks.
    /// </summary>
    public sealed class Healer : Component
    {
        #region Fields
        public static readonly Stat SpeedStat = new Stat(typeof(Healer), StatType.Real, "Speed");
        public static readonly Stat RangeStat = new Stat(typeof(Healer), StatType.Real, "Range");

        private float speed = 1;
        private float range = 1;
        #endregion

        #region Constructors
        public Healer(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the speed at which the host <see cref="Entity"/> heals.
        /// </summary>
        [Mandatory]
        public float Speed
        {
            get { return speed; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Speed");
                speed = value;
            }
        }

        /// <summary>
        /// Accesses the healing range of the host <see cref="Entity"/>.
        /// </summary>
        [Mandatory]
        public float Range
        {
            get { return range; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Range");
                range = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if a given <see cref="Entity"/> is within this <see cref="Entity"/>'s healing range.
        /// </summary>
        /// <param name="target">The target <see cref="Entity"/>.</param>
        /// <returns>A value indicating if <paramref name="target"/> is in the healing range.</returns>
        public bool IsInRange(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Spatial spatial = Entity.Spatial;
            float range = (float)Entity.GetStatValue(Healer.RangeStat);
            return spatial != null && spatial.IsInRange(target, range);
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
            foreach (Spatial targetSpatial in World.SpatialManager.Intersecting(vision.LineOfSight))
            {
                Entity target = targetSpatial.Entity;
                Health targetHealth = target.Components.TryGet<Health>();
                if (target == Entity
                    || targetHealth == null
                    || targetHealth.Constitution != Constitution.Biological
                    || targetHealth.Damage == 0
                    || !vision.IsInRange(target)
                    || !FactionMembership.IsAlliedTo(Entity, target))
                {
                    continue;
                }

                float score = 50 - Region.Distance(spatial.GridRegion, targetSpatial.GridRegion);
                if (score > bestScore) bestTarget = target;
            }

            return bestTarget;
        }
        #endregion
    }
}
