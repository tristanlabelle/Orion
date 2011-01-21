using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

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
        public Harvest(Entity entity, float speed, int maxCarryingAmount)
            : base(entity)
        {
            this.speed = speed;
            this.maxCarryingAmount = maxCarryingAmount;
        }
        #endregion

        #region Properties
        public float Speed
        {
            get { return speed; }
        }

        public int MaxCarryingAmount
        {
            get { return maxCarryingAmount; }
        }

        public ResourceAmount ResourceAmount
        {
            get { return resourceAmount; }
            set { resourceAmount = value; }
        }
        #endregion
    }
}
