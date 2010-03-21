using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Engine;

namespace Orion.GameLogic.Utilities
{
    /// <summary>
    /// Monitors the death of entities and keeps track of their ruins and skeletons.
    /// </summary>
    public sealed class RuinsMonitor
    {
        #region Fields
        private static readonly float buildingRuinLifeSpan = 60 * 4;
        private static readonly float unitRuinLifeSpan = 60;

        private readonly World world;
        private readonly List<Ruin> ruins = new List<Ruin>();
        private readonly ReadOnlyCollection<Ruin> readOnlyRuins;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new ruins monitor from the world to be monitored.
        /// </summary>
        /// <param name="world">The world to be monitored for deaths.</param>
        public RuinsMonitor(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.readOnlyRuins = new ReadOnlyCollection<Ruin>(ruins);

            this.world.Updated += OnWorldUpdated;
            this.world.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of ruins currently present on the world.
        /// </summary>
        public ReadOnlyCollection<Ruin> Ruins
        {
            get { return readOnlyRuins; }
        }
        #endregion

        #region Methods
        private void OnWorldUpdated(World sender, SimulationStep step)
        {
            for (int i = ruins.Count - 1; i >= 0; --i)
            {
                Ruin ruin = ruins[i];
                ruin.Update(step.TimeDeltaInSeconds);
                if (ruin.IsDead) ruins.RemoveAt(i);
            }
        }

        private void OnEntityRemoved(EntityManager sender, Entity args)
        {
            Unit unit = args as Unit;
            if (unit == null) return;

            float lifeSpan = unit.IsBuilding ? buildingRuinLifeSpan : unitRuinLifeSpan;
            Ruin ruin = new Ruin(unit.Faction, unit.Type, unit.Center, lifeSpan);
            ruins.Add(ruin);
        }
        #endregion
    }
}
