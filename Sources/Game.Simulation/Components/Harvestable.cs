using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Harvestable : Component
    {
        #region Fields
        public static readonly Stat HarvestRateStat = new Stat(typeof(Harvestable), StatType.Real, "HarvestRate");

        private int resourceAmount;
        private ResourceType type;
        #endregion

        #region Constructors
        public Harvestable(Entity e)
            : base(e)
        { }
        #endregion

        #region Events
        public event Action<Entity> RemainingAmountChanged;
        #endregion

        #region Properties
        [Mandatory]
        public int AmountRemaining
        {
            get { return resourceAmount; }
            set
            {
                resourceAmount = value;
                RemainingAmountChanged.Raise(Entity);
            }
        }

        [Mandatory]
        public ResourceType Type
        {
            get { return type; }
            set { type = value; }
        }

        [Transient]
        public bool IsEmpty
        {
            get { return resourceAmount == 0; }
        }
        #endregion

        #region Methods
        public void Harvest(int amount)
        {
            AmountRemaining -= amount;
        }
        #endregion
    }
}
