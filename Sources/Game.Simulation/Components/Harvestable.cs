using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A component which enables <see cref="Entity">entities</see> to
    /// be harvested for resources.
    /// </summary>
    public sealed class Harvestable : Component
    {
        #region Fields
        private int amount = 1;
        private ResourceType type;
        #endregion

        #region Constructors
        public Harvestable(Entity entity) : base(entity) { }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the amount of resources available in this node changes.
        /// </summary>
        public event Action<Entity> AmountChanged;
        #endregion

        #region Properties
        [Mandatory]
        public int Amount
        {
            get { return amount; }
            set
            {
                Argument.EnsurePositive(value, "Amount");
                if (value == amount) return;

                amount = value;
                AmountChanged.Raise(Entity);
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
            get { return amount == 0; }
        }
        #endregion

        #region Methods
        public void Harvest(int amount)
        {
            Amount = Math.Max(0, this.amount - amount);
        }
        #endregion
    }
}
