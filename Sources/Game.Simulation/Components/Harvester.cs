using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to take part in resource harvesting tasks.
    /// </summary>
    public sealed class Harvester : Component
    {
        #region Fields
        public static readonly Stat SpeedStat = new Stat(typeof(Harvester), StatType.Real, "Speed");
        public static readonly Stat MaxCarryingAmountStat = new Stat(typeof(Harvester), StatType.Integer, "MaxCarryingAmount");

        private float speed = 1;
        private int maxCarryingAmount = 1;

        private ResourceAmount transportedAmount;
        #endregion

        #region Constructor
        public Harvester(Entity entity) : base(entity) { }
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
            set
            {
                Argument.EnsurePositive(value, "MaxCarryingAmount");
                maxCarryingAmount = value;
            }
        }

        public ResourceAmount TransportedAmount
        {
            get { return transportedAmount; }
            set { transportedAmount = value; }
        }
        #endregion
    }
}
