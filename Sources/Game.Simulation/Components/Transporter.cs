using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Enables an <see cref="Entity"/> to transport other <see cref="Entities"/>.
    /// </summary>
    public sealed class Transporter : Component
    {
        #region Fields
        public static readonly Stat CapacityStat = new Stat(typeof(Transporter), StatType.Integer, "Capacity");

        private int capacity = 1;
        private readonly List<Entity> passengers = new List<Entity>();
        private readonly List<Spatial> positionComponents = new List<Spatial>();
        #endregion

        #region Constructors
        public Transporter(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public int Capacity
        {
            get { return capacity; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Capacity");
                capacity = value;
            }
        }

        [Transient]
        public int LoadSize
        {
            get { return passengers.Sum(entity => Cost.GetResourceAmount(entity).Food); }
        }

        [Transient]
        public int RemainingSpace
        {
            get { return capacity - LoadSize; }
        }

        [Transient]
        public IEnumerable<Entity> Passengers
        {
            get { return passengers; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if a given <see cref="Entity"/> can be embarked in this transport.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if the <see cref="Entity"/> can be embarked.</returns>
        public bool CanEmbark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!entity.Components.Has<Mobile>() || entity.Components.Has<Transporter>()) return false;

            Spatial embarkeeSpatial = entity.Spatial;
            if (embarkeeSpatial == null || embarkeeSpatial.CollisionLayer != CollisionLayer.Ground) return false;

            FactionMembership embarkeeMembership = entity.Components.TryGet<FactionMembership>();
            if (embarkeeMembership == null) return false;

            FactionMembership embarkerMembership = entity.Components.TryGet<FactionMembership>();
            ResourceAmount embarkeeCost = Cost.GetResourceAmount(entity);
            if (embarkerMembership == null)
                return RemainingSpace >= embarkeeCost.Food;

            return (embarkerMembership == null || embarkerMembership.Faction == embarkeeMembership.Faction)
                && RemainingSpace >= embarkeeCost.Food;
        }

        /// <summary>
        /// Tests if the receiver's <see cref="Entity"/> is within sqrt(2) squares of the argument entity.
        /// </summary>
        /// <param name="target">The entity whose proximity needs verification</param>
        /// <returns>True if the target is within sqrt(2) squares distance of the receiver's <see cref="Entity"/></returns>
        public bool IsInRange(Entity target)
        {
            Spatial targetSpatial = target.Components.TryGet<Spatial>();
            if (targetSpatial == null) return false;

            Spatial selfSpatial = Entity.Components.TryGet<Spatial>();
            if (selfSpatial == null) return false;
            return selfSpatial.IsInRange(target, (float)Math.Sqrt(2));
        }

        /// <summary>
        /// Embarks a given <see cref="Entity"/> in this transport.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be embarked.</param>
        public void Embark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!CanEmbark(entity))
                throw new ArgumentException("entity");

            Spatial embarkeePosition = entity.Spatial;
            entity.Components.Remove<Spatial>();
            TaskQueue queue = entity.Components.TryGet<TaskQueue>();
            if (queue != null) queue.Clear();

            positionComponents.Add(embarkeePosition);
            passengers.Add(entity);
        }

        /// <summary>
        /// Disembarks a given <see cref="Entity"/> from this transport.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be disembarked.</param>
        public void Disembark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!passengers.Contains(entity))
                throw new ArgumentException("entity");

            Spatial transporterSpatial = Entity.Spatial;
            Spatial passengerSpatial = GetSpatialComponentOfPassenger(entity);
            Point? location = transporterSpatial
                .GridRegion.Points
                .Concat(transporterSpatial.GridRegion.GetAdjacentPoints())
                .FirstOrNull(point => Entity.World.IsFree(point, passengerSpatial.CollisionLayer));

            if (!location.HasValue)
            {
                Entity.RaiseWarning("Pas de place pour le débarquement d'unités");
                return;
            }

            passengers.Remove(entity);
            positionComponents.Remove(passengerSpatial);

            entity.Components.Add(passengerSpatial);
            passengerSpatial.Position = location.Value;
        }

        /// <summary>
        /// Accesses the <see cref="Spatial"/> component of a passenger <see cref="Entity"/>.
        /// This is particularly useful to know the size of a passenger before dropping it to the
        /// ground.
        /// </summary>
        /// <param name="target">The passenger <see cref="Entity"/></param>
        /// <returns>The <see cref="Spatial"/> component of the passenger</returns>
        /// <exception cref="ArgumentException">If the passenger does not belong to this component</exception>
        private Spatial GetSpatialComponentOfPassenger(Entity target)
        {
            int index = passengers.IndexOf(target);
            if (index == -1)
                throw new ArgumentException("Target does not belong to this transporter", "target");

            return positionComponents[index];
        }
        #endregion
    }
}
