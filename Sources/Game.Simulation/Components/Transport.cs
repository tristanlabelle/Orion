using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation.Components
{
    public class Transport : Component
    {
        #region Fields
        public static readonly EntityStat<int> CapacityStat = new EntityStat<int>(typeof(Transport), "Capacity", "Capacité de transport");
        public static readonly EntityStat<float> EmbarkSpeedStat = new EntityStat<float>(typeof(Transport), "EmbarkSpeed", "Vitesse d'embarquement");
        public static readonly EntityStat<float> DisembarkSpeedStat = new EntityStat<float>(typeof(Transport), "DisembarkSpeed", "Vitesse de débarquement");

        private int capacity;
        private float embarkSpeed;
        private float disembarkSpeed;
        private float lastEmbarkTime;
        private float lastDisembarkTime;
        private List<Entity> embarkedUnits = new List<Entity>();
        private List<Position> positionComponents = new List<Position>();
        #endregion

        #region Constructors
        public Transport(Entity entity, float embarkSpeed, float disembarkSpeed, int capacity)
            : base(entity)
        {
            this.embarkSpeed = embarkSpeed;
            this.disembarkSpeed = disembarkSpeed;
            this.capacity = capacity;
        }
        #endregion

        #region Properties
        public int Capacity
        {
            get { return capacity; }
        }

        public float EmbarkSpeed
        {
            get { return embarkSpeed; }
        }

        public float DisembarkSpeed
        {
            get { return disembarkSpeed; }
        }

        public float LastEmbarkTime
        {
            get { return lastEmbarkTime; }
            set { lastEmbarkTime = value; }
        }

        public float LastDisembarkTime
        {
            get { return lastDisembarkTime; }
            set { lastDisembarkTime = value; }
        }

        public int Load
        {
            get
            {
                return embarkedUnits
                    .Select(e => e.GetComponent<FactionMembership>())
                    .Sum(c => c.FoodRequirement);
            }
        }

        public int RemainingSpace
        {
            get { return capacity - Load; }
        }

        public IEnumerable<Entity> EmbarkedEntities
        {
            get { return embarkedUnits; }
        }
        #endregion

        #region Methods
        public bool CanEmbark(Entity entity)
        {
            Move mobility = entity.GetComponentOrNull<Move>();
            if (mobility == null) return false;

            Position embarkeePosition = entity.GetComponentOrNull<Position>();
            if (embarkeePosition == null) return false;

            FactionMembership embarkeeMembership = entity.GetComponentOrNull<FactionMembership>();
            if (embarkeeMembership == null) return false;

            FactionMembership embarkerMembership = entity.GetComponentOrNull<FactionMembership>();

            if (embarkerMembership == null)
                return RemainingSpace <= embarkeeMembership.FoodRequirement;

            return (embarkerMembership == null || embarkerMembership.Faction == embarkeeMembership.Faction)
                && RemainingSpace <= embarkeeMembership.FoodRequirement;
        }

        public void Embark(Entity entity)
        {
            if (!CanEmbark(entity))
                throw new ArgumentException("entity");

            Position embarkeePosition = entity.GetComponent<Position>();
            entity.RemoveComponent<Position>();

            positionComponents.Add(embarkeePosition);
            embarkedUnits.Add(entity);
        }

        public void Disembark(Entity entity)
        {
            if (!embarkedUnits.Contains(entity))
                throw new ArgumentException("entity");

            Position embarkerPosition = Entity.GetComponent<Position>();
            Position embarkeePosition = entity.GetComponent<Position>();
            Point? location = embarkerPosition
                .GridRegion.Points
                .Concat(embarkerPosition.GridRegion.GetAdjacentPoints())
                .FirstOrNull(point => Entity.World.IsFree(point, embarkeePosition.CollisionLayer));

            if (!location.HasValue)
            {
                Entity.RaiseWarning("Pas de place pour le débarquement d'unités");
                return;
            }

            int embarkeeIndex = embarkedUnits.IndexOf(entity);
            Position position = positionComponents[embarkeeIndex];
            embarkedUnits.RemoveAt(embarkeeIndex);
            positionComponents.RemoveAt(embarkeeIndex);

            position.Location = location.Value;
            entity.AddComponent(position);
        }
        #endregion
    }
}
