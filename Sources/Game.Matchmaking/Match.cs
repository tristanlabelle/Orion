using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using System.Diagnostics;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Represents a match that is played between a number of commanders in a world.
    /// </summary>
    public sealed class Match
    {
        #region Fields
        private const float defaultRandomHeroProbability = 0.015f;

        private readonly World world;
        private readonly Random random;
        private readonly Func<Point, bool> canBuildPredicate;
        private readonly UnitTypeRegistry unitTypes = new UnitTypeRegistry();
        private readonly TechnologyTree technologyTree = new TechnologyTree();
        private bool isRunning = true;
        private bool areRandomHeroesEnabled = true;
        #endregion

        #region Constructors
        public Match(World world, Random random, Func<Point, bool> canBuildPredicate)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(random, "random");

            this.world = world;
            this.random = random;
            this.canBuildPredicate = canBuildPredicate;
        }

        public Match(World world, Random random)
            : this(world, random, null) { }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a message was received from a <see cref="Faction"/>.
        /// </summary>
        public event Action<Match, FactionMessage> FactionMessageReceived;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the simulation world used by this match.
        /// </summary>
        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Gets the collection of unit types available in this match.
        /// </summary>
        public UnitTypeRegistry UnitTypes
        {
            get { return unitTypes; }
        }

        /// <summary>
        /// Gets the technology tree which provides the technologies available in this match.
        /// </summary>
        public TechnologyTree TechnologyTree
        {
            get { return technologyTree; }
        }

        /// <summary>
        /// Gets a value indicating if the match is currently updating.
        /// </summary>
        public bool IsRunning
        {
            get { return isRunning; }
        }

        /// <summary>
        /// Accesses a value indicating if random heroes are enabled.
        /// </summary>
        public bool AreRandomHeroesEnabled
        {
            get { return areRandomHeroesEnabled; }
            set { areRandomHeroesEnabled = value; }
        }

        /// <summary>
        /// Gets the probabily to get a hero when training a unit.
        /// </summary>
        public float RandomHeroProbability
        {
            get { return areRandomHeroesEnabled ? defaultRandomHeroProbability : 0; }
        }

        /// <summary>
        /// Gets the random number generator used in this match.
        /// </summary>
        /// <remarks>
        /// Internal to minimize chances of invalid usage outside the matchmaking logic.
        /// </remarks>
        internal Random Random
        {
            get { return random; }
        }
        #endregion

        #region Methods
        public void Pause()
        {
            isRunning = false;
        }

        public void Resume()
        {
            isRunning = true;
        }

        public bool CanBuild(Region region)
        {
            return world.IsFree(region, CollisionLayer.Ground)
                && (canBuildPredicate == null || region.Points.All(p => canBuildPredicate(p)));
        }

        /// <summary>
        /// Gets a random hero unit type from a given unit type.
        /// </summary>
        /// <param name="unitType">The original unit type.</param>
        /// <returns>One of the heroes of the original unit type, or that unit type itself.</returns>
        public UnitType RandomizeHero(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            if (!areRandomHeroesEnabled) return unitType;

            while (true)
            {
                var heroUpgrades = unitType.Upgrades
                    .Where(u => u.AladdiumCost == 0 && u.AlageneCost == 0);

                int upgradeCount = heroUpgrades.Count();
                if (upgradeCount == 0 || random.NextDouble() >= RandomHeroProbability)
                    break;

                int upgradeIndex = random.Next(upgradeCount);
                UnitTypeUpgrade upgrade = heroUpgrades.ElementAt(upgradeIndex);

                UnitType heroUnitType = UnitTypes.FromName(upgrade.Target);
                if (heroUnitType == null)
                {
#if DEBUG
                    Debug.Fail("Failed to retreive hero unit type {0} for unit type {1}."
                        .FormatInvariant(upgrade.Target, unitType.Name));
#endif
                    break;
                }

                unitType = heroUnitType;
            }

            return unitType;
        }

        /// <summary>
        /// Notifies this <see cref="Match"/> that a message from a <see cref="Faction"/> has been received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void Post(FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");
            FactionMessageReceived.Raise(this, message);
        }
        #endregion
    }
}
