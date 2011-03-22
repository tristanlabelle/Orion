using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
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
        private readonly PrototypeRegistry prototypes;
        private readonly TechnologyTree technologyTree;
        private bool isRunning = true;
        private bool areRandomHeroesEnabled = true;
        #endregion

        #region Constructors
        public Match(AssetsDirectory assets, World world, Random random)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(assets, "assets");
            Argument.EnsureNotNull(random, "random");

            this.prototypes = new PrototypeRegistry(assets);
            this.technologyTree = new TechnologyTree(assets);
            this.world = world;
            this.random = random;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when one of this <see cref="World"/>'s <see cref="Faction"/>s uses a cheat code.
        /// The first argument specifies the faction that used the cheat.
        /// The second argument specifies the cheat that was used.
        /// </summary>
        public event Action<Match, Faction, string> CheatUsed;
        
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
        /// Gets the collection of <see cref="Entity"/> prototypes available in this match.
        /// </summary>
        public PrototypeRegistry Prototypes
        {
            get { return prototypes; }
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
        
        /// <summary>
        /// Raises the <see cref="E:CheatUsed"/> event.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> that cheated.</param>
        /// <param name="cheat">The cheat that was used.</param>
        public void RaiseCheatUsed(Faction faction, string cheat)
        {
        	CheatUsed.Raise(this, faction, cheat);
        }

        /// <summary>
        /// Gets a random hero unit type from a given prototype.
        /// </summary>
        /// <param name="prototype">The original protottype.</param>
        /// <returns>One of the heroes of the original unit type, or that prototype itself.</returns>
        public Entity RandomizeHero(Entity prototype)
        {
            Argument.EnsureNotNull(prototype, "unitType");
            if (!areRandomHeroesEnabled) return prototype;

            while (true)
            {
                var heroUpgrades = prototype.Identity.Upgrades
                    .Where(u => u.AladdiumCost == 0 && u.AlageneCost == 0);

                int upgradeCount = heroUpgrades.Count();
                if (upgradeCount == 0 || random.NextDouble() >= RandomHeroProbability)
                    break;

                int upgradeIndex = random.Next(upgradeCount);
                EntityUpgrade upgrade = heroUpgrades.ElementAt(upgradeIndex);

                Entity heroPrototype = Prototypes.FromName(upgrade.Target);
                if (heroPrototype == null)
                {
                    Debug.Fail("Failed to retreive hero unit type {0} for unit type {1}."
                        .FormatInvariant(upgrade.Target, prototype.Identity.Name));
                    break;
                }

                prototype = heroPrototype;
            }

            return prototype;
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
