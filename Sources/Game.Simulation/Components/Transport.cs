using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public class Transport : Component
    {
        #region Fields
        public static readonly EntityStat CapacityStat = new EntityStat(typeof(Transport), "Capacity", "Capacité de transport");

        private int capacity;
        private List<Entity> embarkedUnits = new List<Entity>();
        private List<Position> positionComponents = new List<Position>();
        #endregion

        #region Constructors
        public Transport(Entity entity, int capacity)
            : base(entity)
        {
            this.capacity = capacity;
        }
        #endregion

        #region Properties
        public int Capacity
        {
            get { return capacity; }
        }

        public int Load
        {
            get
            {
                return embarkedUnits
                    .Select(e => e.GetComponent<Faction>())
                    .Sum(c => c.FoodRequirement);
            }
        }

        public int RemainingSpace
        {
            get { return capacity - Load; }
        }
        #endregion

        #region Methods
        public bool CanEmbark(Entity entity)
        {
            Move mobility = entity.GetComponentOrNull<Move>();
            if (mobility == null) return false;

            Faction embarkeeMembership = entity.GetComponentOrNull<Faction>();
            if (embarkeeMembership == null) return false;

            Faction embarkerMembership = entity.GetComponentOrNull<Faction>();

            if (embarkerMembership == null)
                return RemainingSpace <= embarkeeMembership.FoodRequirement;

            return (embarkerMembership == null || embarkerMembership.Faction == embarkeeMembership.Faction)
                && RemainingSpace <= embarkeeMembership.FoodRequirement;
        }

        public void Embark(Entity entity)
        {
            if (!CanEmbark(entity))
                throw new ArgumentException("entity");

            embarkedUnits.Add(entity);
        }
        #endregion
    }
}
