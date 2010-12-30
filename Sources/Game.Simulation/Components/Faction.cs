using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameFaction = Orion.Game.Simulation.Faction;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Indicates membership of a faction, and all what it implies.
    /// </summary>
    public class Faction : Component
    {
        #region Fields
        private GameFaction faction;
        private int foodRequirement;
        #endregion

        #region Constructors
        public Faction(Entity entity, GameFaction faction, int foodRequirement)
            : base(entity)
        {
            this.faction = faction;
            this.foodRequirement = foodRequirement;
        }
        #endregion

        #region Properties
        public GameFaction Faction
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
