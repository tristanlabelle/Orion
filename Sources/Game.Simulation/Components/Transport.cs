using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Transport : Component
    {
        #region Fields
        public static readonly Stat CapacityStat = new Stat(typeof(Transport), StatType.Integer, "Capacity");
        public static readonly Stat EmbarkSpeedStat = new Stat(typeof(Transport), StatType.Real, "EmbarkSpeed");
        public static readonly Stat DisembarkSpeedStat = new Stat(typeof(Transport), StatType.Real, "DisembarkSpeed");

        private int capacity;
        private float loadSpeed;
        private float unloadSpeed;

        private float lastLoadTime;
        private float lastUnloadTime;
        private List<Entity> loadedUnits = new List<Entity>();
        private List<Spatial> positionComponents = new List<Spatial>();
        #endregion

        #region Constructors
        public Transport(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public int Capacity
        {
            get { return capacity; }
            set { capacity = value; }
        }

        [Mandatory]
        public float LoadSpeed
        {
            get { return loadSpeed; }
            set { loadSpeed = value; }
        }

        [Mandatory]
        public float UnloadSpeed
        {
            get { return unloadSpeed; }
            set { unloadSpeed = value; }
        }

        public float LastLoadTime
        {
            get { return lastLoadTime; }
            set { lastLoadTime = value; }
        }

        public float LastUnloadTime
        {
            get { return lastUnloadTime; }
            set { lastUnloadTime = value; }
        }

        [Transient]
        public int LoadSize
        {
            get
            {
                return loadedUnits
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
        public IEnumerable<Entity> LoadedEntities
        {
            get { return loadedUnits; }
        }
        #endregion

        #region Methods
        public bool CanEmbark(Entity entity)
        {
            Move mobility = entity.Components.TryGet<Move>();
            if (mobility == null) return false;

            Spatial embarkeePosition = entity.Components.TryGet<Spatial>();
            if (embarkeePosition == null) return false;

            FactionMembership embarkeeMembership = entity.Components.TryGet<FactionMembership>();
            if (embarkeeMembership == null) return false;

            FactionMembership embarkerMembership = entity.Components.TryGet<FactionMembership>();

            if (embarkerMembership == null)
                return RemainingSpace <= embarkeeMembership.FoodRequirement;

            return (embarkerMembership == null || embarkerMembership.Faction == embarkeeMembership.Faction)
                && RemainingSpace <= embarkeeMembership.FoodRequirement;
        }

        public void Load(Entity entity)
        {
            if (!CanEmbark(entity))
                throw new ArgumentException("entity");

            Spatial embarkeePosition = entity.Components.Get<Spatial>();
            entity.Components.Remove<Spatial>();

            positionComponents.Add(embarkeePosition);
            loadedUnits.Add(entity);
        }

        public void Unload(Entity entity)
        {
            if (!loadedUnits.Contains(entity))
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

            int embarkeeIndex = loadedUnits.IndexOf(entity);
            Spatial position = positionComponents[embarkeeIndex];
            loadedUnits.RemoveAt(embarkeeIndex);
            positionComponents.RemoveAt(embarkeeIndex);

            position.Position = location.Value;
            entity.Components.Add(position);
        }
        #endregion
    }
}
