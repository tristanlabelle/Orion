using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Harvest : Component
    {
        #region Fields
        public static readonly EntityStat SpeedStat = new EntityStat(typeof(Harvest), StatType.Real, "Speed", "Vitesse de récolte");
        public static readonly EntityStat MaxCarryingAmountStat = new EntityStat(typeof(Harvest), StatType.Integer, "MaxCarryingAmount", "Quantité maximum");

        private float speed;
        private int maxCarryingAmount;

        private ResourceAmount resourceAmount;
        #endregion

        #region Constructor
        public Harvest(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        [Mandatory]
        public int MaxCarryingAmount
        {
            get { return maxCarryingAmount; }
            set { maxCarryingAmount = value; }
        }

        public ResourceAmount ResourceAmount
        {
            get { return resourceAmount; }
            set { resourceAmount = value; }
        }
        #endregion
    }
}
