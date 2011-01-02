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
        #endregion

        #region Constructors
        public FactionMembership(Entity entity, Faction faction, int foodRequirement)
            : base(entity)
        {
            this.faction = faction;
            this.foodRequirement = foodRequirement;
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
        #endregion
    }
}
