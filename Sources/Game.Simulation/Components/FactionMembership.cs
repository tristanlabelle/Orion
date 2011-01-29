using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components.Serialization;

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
        public FactionMembership(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        [Mandatory]
        public int FoodRequirement
        {
            get { return foodRequirement; }
            set { foodRequirement = value; }
        }

        [Persistent]
        public int FoodProvided
        {
            get { return foodProvided; }
            set { foodProvided = value; }
        }
        #endregion
    }
}
