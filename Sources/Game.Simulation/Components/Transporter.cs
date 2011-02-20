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
            get
            {
                return passengers
                    .Select(e => e.Components.Get<FactionMembership>())
                    .Sum(c => c.FoodRequirement);
            }
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
        public bool CanEmbark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!entity.Components.Has<Move>() || entity.Components.Has<Transporter>()) return false;

            Spatial embarkeeSpatial = entity.Components.TryGet<Spatial>();
            if (embarkeeSpatial == null || embarkeeSpatial.CollisionLayer != CollisionLayer.Ground) return false;

            FactionMembership embarkeeMembership = entity.Components.TryGet<FactionMembership>();
            if (embarkeeMembership == null) return false;

            FactionMembership embarkerMembership = entity.Components.TryGet<FactionMembership>();

            if (embarkerMembership == null)
                return RemainingSpace <= embarkeeMembership.FoodRequirement;

            return (embarkerMembership == null || embarkerMembership.Faction == embarkeeMembership.Faction)
                && RemainingSpace <= embarkeeMembership.FoodRequirement;
        }

        public void Embark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!CanEmbark(entity))
                throw new ArgumentException("entity");

            Spatial embarkeePosition = entity.Components.Get<Spatial>();
            entity.Components.Remove<Spatial>();

            positionComponents.Add(embarkeePosition);
            passengers.Add(entity);
        }

        public void Disembark(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            if (!passengers.Contains(entity))
                throw new ArgumentException("entity");

            Spatial embarkerPosition = Entity.Components.Get<Spatial>();
            Spatial embarkeePosition = entity.Components.Get<Spatial>();
            Point? location = embarkerPosition
                .GridRegion.Points
                .Concat(embarkerPosition.GridRegion.GetAdjacentPoints())
                .FirstOrNull(point => Entity.World.IsFree(point, embarkeePosition.CollisionLayer));

            if (!location.HasValue)
            {
                Entity.RaiseWarning("Pas de place pour le débarquement d'unités");
                return;
            }

            int embarkeeIndex = passengers.IndexOf(entity);
            Spatial position = positionComponents[embarkeeIndex];
            passengers.RemoveAt(embarkeeIndex);
            positionComponents.RemoveAt(embarkeeIndex);

            position.Position = location.Value;
            entity.Components.Add(position);
        }
        #endregion
    }
}
