using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Indicates membership of a faction, and all what it implies.
    /// </summary>
    public sealed class FactionMembership : Component
    {
        #region Fields
        public static readonly Stat FoodCostStat = new Stat(typeof(FactionMembership), StatType.Integer, "FoodCost");
        public static readonly Stat ProvidedFoodStat = new Stat(typeof(FactionMembership), StatType.Integer, "ProvidedFood");

        private Faction faction;
        private int foodCost;
        private int providedFood;
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
        public int FoodCost
        {
            get { return foodCost; }
            set
            {
                Argument.EnsurePositive(value, "FoodCost");
                foodCost = value; }
        }

        [Persistent]
        public int ProvidedFood
        {
            get { return providedFood; }
            set
            {
                Argument.EnsurePositive(value, "ProvidedFood");
                providedFood = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the <see cref="Faction"/> of a given <see cref="Entity"/>, if any.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> for which the <see cref="Faction"/> is requested.</param>
        /// <returns>The <see cref="Faction"/> of <paramref name="entity"/>, or <c>null</c> if it has none.</returns>
        public static Faction GetFaction(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            FactionMembership factionMembership = entity.Components.TryGet<FactionMembership>();
            return factionMembership == null ? null : factionMembership.Faction;
        }
        #endregion
    }
}
