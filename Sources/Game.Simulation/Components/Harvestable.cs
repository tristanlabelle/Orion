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

        #region Properties
        [Persistent(true)]
        public int Amount
        {
            get { return amount; }
            set
            {
                Argument.EnsurePositive(value, "Amount");
                amount = value;
            }
        }

        /// <summary>
        /// Accesses the type of resource provided by this <see cref="Harvestable"/>.
        /// </summary>
        [Persistent(true)]
        public ResourceType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets a value indicating if the resources in this entity have been depleted.
        /// </summary>
        public bool IsEmpty
        {
            get { return amount == 0; }
        }
        #endregion

        #region Methods
        public override int GetStateHashCode()
        {
            return amount ^ (int)type;
        }

        /// <summary>
        /// Removes resources from this <see cref="Entity"/>.
        /// </summary>
        /// <param name="amount">The amount of resources to be removed.</param>
        /// <returns>A value indicating if this <see cref="Entity"/> has been depleted.</returns>
        public bool Extract(int amount)
        {
            Argument.EnsurePositive(amount, "amount");
            if (amount > this.amount) throw new ArgumentException("Cannot extract more than the remaining amount.", "Amount");

            this.amount -= amount;
            if (!IsEmpty) return false;
            
            Entity.Die();
            return true;
        }
        #endregion
    }
}
