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
        public static readonly Stat SpeedStat = new Stat(typeof(Harvest), StatType.Real, "Speed");
        public static readonly Stat MaxCarryingAmountStat = new Stat(typeof(Harvest), StatType.Integer, "MaxCarryingAmount");

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
