using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Provides an <see cref="Entity"/> with the capability to build structures.
    /// </summary>
    public sealed class Builder : Component
    {
        #region Fields
        public static readonly Stat SpeedStat = new Stat(typeof(Builder), StatType.Real, "Speed");

        private readonly HashSet<string> buildableTypes = new HashSet<string>();
        private float speed = 1;
        #endregion

        #region Constructors
        public Builder(Entity entity) : base(entity) { }
        #endregion

        #region Properties
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

        [Persistent]
        public ICollection<string> BuildableTypes
        {
            get { return buildableTypes; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if an <see cref="Entity"/> described by a given prototype can be built by the host <see cref="Entity"/>.
        /// </summary>
        /// <param name="prototype">The <see cref="Entity"/> prototype.</param>
        /// <returns>A value indicating if such <see cref="Entity"/> instances can be built.</returns>
        public bool Supports(Entity prototype)
        {
            Argument.EnsureNotNull(prototype, "prototype");
            return buildableTypes.Contains(prototype.Components.Get<Identity>().Name);
        }

        /// <summary>
        /// Finds an <see cref="Entity"/> in need of reparation within this <see cref="Entity"/>'s sight range.
        /// </summary>
        /// <returns>An <see cref="Entity"/> to repair, or <c>null</c> if no appropriate targets are visible.</returns>
        public Entity FindVisibleRepairTarget()
        {
            Spatial spatial = Entity.Spatial;
            Vision vision = Entity.Components.TryGet<Vision>();
            if (spatial == null || vision == null) return null;

            Entity bestTarget = null;
            float bestScore = float.NegativeInfinity;
            foreach (Entity target in World.Entities.Intersecting(vision.LineOfSight))
            {
                Unit targetUnit = target as Unit;
                Spatial targetSpatial = target.Spatial;
                Health targetHealth = target.Components.TryGet<Health>();
                if (target == Entity
                    || targetUnit == null
                    || targetSpatial == null
                    || targetHealth == null
                    || !targetUnit.IsBuilding
                    || targetHealth.Damage == 0
                    || !vision.CanSee(target)
                    || !FactionMembership.IsAlliedTo(Entity, target))
                {
                    continue;
                }

                float score = Region.Distance(spatial.GridRegion, targetSpatial.GridRegion);
                if (score > bestScore) bestTarget = target;
            }

            return bestTarget;
        }
        #endregion
    }
}
