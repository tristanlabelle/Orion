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
        private readonly Dictionary<Entity, Spatial> passengers = new Dictionary<Entity, Spatial>();
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
            get { return passengers.Keys.Sum(entity => Cost.GetResourceAmount(entity).Food); }
        }

        [Transient]
        public int RemainingSpace
        {
            get { return (int)Entity.GetStatValue(CapacityStat) - LoadSize; }
        }

        [Transient]
        public IEnumerable<Entity> Passengers
        {
            get { return passengers.Keys; }
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
        /// <param name="passenger">The <see cref="Entity"/> to be embarked.</param>
        public void Embark(Entity passenger)
        {
            Argument.EnsureNotNull(passenger, "passenger");

            if (!CanEmbark(passenger))
                throw new ArgumentException("Cannot embark passenger.", "passenger");

            TaskQueue passengerTaskQueue = passenger.Components.TryGet<TaskQueue>();
            if (passengerTaskQueue != null) passengerTaskQueue.Clear();

            Spatial passengerSpatial = passenger.Spatial;
            passenger.Components.Remove<Spatial>();

            passengers.Add(passenger, passengerSpatial);
        }

        /// <summary>
        /// Disembarks a given <see cref="Entity"/> from this transport.
        /// </summary>
        /// <param name="passenger">The <see cref="Entity"/> to be disembarked.</param>
        public void Disembark(Entity passenger)
        {
            Argument.EnsureNotNull(passenger, "passenger");

            Spatial passengerSpatial;
            if (!passengers.TryGetValue(passenger, out passengerSpatial))
                throw new KeyNotFoundException("Passenger is not being transported.");

            Spatial transporterSpatial = Entity.Spatial;
            Point? location = transporterSpatial
                .GridRegion.Points
                .Concat(transporterSpatial.GridRegion.GetAdjacentPoints())
                .FirstOrNull(point => Entity.World.IsFree(point, passengerSpatial.CollisionLayer));

            if (!location.HasValue)
            {
                Entity.RaiseWarning("Pas de place pour le débarquement d'unités");
                return;
            }

            passengers.Remove(passenger);

            passenger.Components.Add(passengerSpatial);
            passengerSpatial.Position = location.Value;
        }
        #endregion
    }
}
