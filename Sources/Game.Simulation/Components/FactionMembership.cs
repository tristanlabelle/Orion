using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to be affiliated to a <see cref="Faction"/>,
    /// and affected by its technologies.
    /// </summary>
    public sealed class FactionMembership : Component
    {
        #region Fields
        public static readonly Stat ProvidedFoodStat = new Stat(typeof(FactionMembership), StatType.Integer, "ProvidedFood");

        private Faction faction;
        private bool isKeepAlive = true;
        private int providedFood;
        private Faction.FoodToken providedFoodToken;
        #endregion

        #region Constructors
        public FactionMembership(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="Faction"/>
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
            set
            {
                if (value == faction) return;

                Debug.Assert(faction == null,
                    "Warning: an entity is changing faction membership, "
                    + "this might cause lots of issues with technologies, fog of war, etc. "
                    + "If the feature has been properly implemented, this assert can be removed.");
                faction = value;
            }
        }

        /// <summary>
        /// Accesses a value indicating if this <see cref="Entity"/> keeps
        /// its <see cref="Faction"/> alive.
        /// </summary>
        public bool IsKeepAlive
        {
            get { return isKeepAlive; }
            set { isKeepAlive = value; }
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
        public override StatValue GetStatBonus(Stat stat)
        {
            StatValue technologyBonuses = faction == null
                ? StatValue.CreateZero(stat.Type)
                : faction.GetTechnologyBonuses(Entity, stat);
            return base.GetStatBonus(stat) + technologyBonuses;
        }

        protected override void Update(SimulationStep step)
        {
            bool underConstruction = Entity.Components.Has<BuildProgress>();
            if (providedFoodToken != null)
            {
                if (faction != providedFoodToken.Faction || underConstruction)
                {
                    providedFoodToken.Dispose();
                    providedFoodToken = null;
                }
                else
                {
                    providedFoodToken.Amount = (int)Entity.GetStatValue(ProvidedFoodStat);
                }
            }

            if (providedFoodToken == null && faction != null && !underConstruction)
            {
                providedFoodToken = faction.CreateFoodToken(Faction.FoodTokenType.Provide, (int)Entity.GetStatValue(ProvidedFoodStat));
            }
        }

        protected override void Sleep()
        {
            if (providedFoodToken != null)
            {
                providedFoodToken.Dispose();
                providedFoodToken = null;
            }
        }

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

        /// <summary>
        /// Tests if an <see cref="Entity"/> is allied to another one. That is,
        /// if they belong to allied factions.
        /// </summary>
        /// <param name="entity">The source <see cref="Entity"/>.</param>
        /// <param name="target">The target <see cref="Entity"/>.</param>
        /// <returns>A value indcating if <paramref name="entity"/> is allied to <paramref name="target"/>.</returns>
        public static bool IsAlliedTo(Entity entity, Entity target)
        {
            Argument.EnsureNotNull(entity, "first");
            Argument.EnsureNotNull(target, "target");
            
            Faction sourceFaction = GetFaction(entity);
            Faction targetFaction = GetFaction(target);

            return sourceFaction != null
                && targetFaction != null
                && sourceFaction.GetDiplomaticStance(targetFaction).HasFlag(DiplomaticStance.AlliedVictory);
        }
        #endregion
    }
}
