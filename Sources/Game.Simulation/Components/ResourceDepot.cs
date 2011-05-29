using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> as being a drop point for harvested resources.
    /// </summary>
    public sealed class ResourceDepot : Component
    {
        #region Fields
        public static readonly Stat TaxStat = new Stat(typeof(ResourceDepot), StatType.Integer, "Tax");

        private int tax;
        #endregion

        #region Constructors
        public ResourceDepot(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the tax which is deduced from the amount of resources
        /// deposited in each deposit.
        /// </summary>
        [Persistent]
        public int Tax
        {
            get { return tax; }
            set
            {
                Argument.EnsurePositive(value, "Tax");
                this.tax = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deposits a given amount of a given resource type in this depot,
        /// applying a tax if there's one.
        /// </summary>
        /// <param name="type">The type of resource to be deposited.</param>
        /// <param name="amount">The amount of resources deposited.</param>
        public void Deposit(ResourceType type, int amount)
        { 
            Faction faction = FactionMembership.GetFaction(Entity);
            if (faction == null) return;

            amount -= (int)Entity.GetStatValue(TaxStat);
            if (amount <= 0) return;

            faction.AddResources(type, amount);
        }

        public override int GetStateHashCode()
        {
            return tax;
        }
        #endregion
    }
}
