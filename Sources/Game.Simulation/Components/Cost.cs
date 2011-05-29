using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// A component which gives a resource cost to an <see cref="Entity"/>.
    /// </summary>
    public sealed class Cost : Component
    {
        #region Fields
        public static readonly Stat AladdiumStat = new Stat(typeof(Cost), StatType.Integer, "Aladdium");
        public static readonly Stat AlageneStat = new Stat(typeof(Cost), StatType.Integer, "Alagene");
        public static readonly Stat FoodStat = new Stat(typeof(Cost), StatType.Integer, "Food");
        public static readonly Stat SpawnTimeStat = new Stat(typeof(Cost), StatType.Real, "SpawnTime");

        private int aladdium;
        private int alagene;
        private int food;
        private float spawnTime = 1;
        private Faction.FoodToken usedFoodToken;
        #endregion

        #region Constructors
        public Cost(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the cost of spawning a <see cref="Entity"/>, in aladdium resource points.
        /// </summary>
        [Persistent]
        public int Aladdium
        {
            get { return aladdium; }
            set
            {
                Argument.EnsurePositive(value, "Aladdium");
                aladdium = value;
            }
        }

        /// <summary>
        /// Accesses the cost of spawning a <see cref="Entity"/>, in alagene resource points.
        /// </summary>
        [Persistent]
        public int Alagene
        {
            get { return alagene; }
            set
            {
                Argument.EnsurePositive(value, "Alagene");
                alagene = value;
            }
        }

        /// <summary>
        /// Accesses the cost of an <see cref="Entity"/>, in food points.
        /// </summary>
        [Persistent]
        public int Food
        {
            get { return food; }
            set
            {
                Argument.EnsurePositive(value, "Food");
                food = value;
            }
        }

        /// <summary>
        /// Accesses the time needed to build or train this <see cref="Entity"/>.
        /// </summary>
        [Persistent]
        public float SpawnTime
        {
            get { return spawnTime; }
            set
            {
                Argument.EnsurePositive(value, "SpawnTime");
                spawnTime = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the amount of resources a given entity costs.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/>.</param>
        /// <returns>Its cost in resources.</returns>
        public static ResourceAmount GetResourceAmount(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Cost cost = entity.Components.TryGet<Cost>();
            return cost == null
                ? ResourceAmount.Zero
                : new ResourceAmount(cost.aladdium, cost.alagene, cost.food);
        }

        public override int GetStateHashCode()
        {
            int hashCode = aladdium ^ alagene ^ food ^ spawnTime.GetHashCode();
            if (usedFoodToken != null)
                hashCode ^= usedFoodToken.Amount ^ usedFoodToken.Type.GetHashCode();
            return hashCode;
        }

        protected override void Update(SimulationStep step)
        {
            Faction faction = FactionMembership.GetFaction(Entity);

            if (usedFoodToken != null)
            {
                if (faction != usedFoodToken.Faction)
                {
                    usedFoodToken.Dispose();
                    usedFoodToken = null;
                }
                else
                {
                    usedFoodToken.Amount = (int)Entity.GetStatValue(FoodStat);
                }
            }

            if (usedFoodToken == null && faction != null)
            {
                usedFoodToken = faction.CreateFoodToken(Faction.FoodTokenType.Use, (int)Entity.GetStatValue(FoodStat));
            }
        }

        protected override void Deactivate()
        {
            if (usedFoodToken != null)
            {
                usedFoodToken.Dispose();
                usedFoodToken = null;
            }
        }
        #endregion
    }
}
