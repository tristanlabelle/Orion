using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Indicates membership of a faction, and all what it implies.
    /// </summary>
    public class FactionMembership : Component
    {
        #region Fields
        private Faction faction;
        private int foodRequirement;
        private int foodProvided;
        #endregion

        #region Constructors
        public FactionMembership(Entity entity, Faction faction, int foodRequirement, int foodProvided)
            : base(entity)
        {
            this.faction = faction;
            this.foodRequirement = foodRequirement;
            this.foodProvided = foodProvided;
        }
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
        }

        public int FoodRequirement
        {
            get { return foodRequirement; }
        }

        public int FoodProvided
        {
            get { return foodProvided; }
        }
        #endregion
    }
}
