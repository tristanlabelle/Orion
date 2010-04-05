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

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Represents a match that is played between a number of commanders in a world.
    /// </summary>
    public sealed class Match
    {
        #region Fields
        private readonly World world;
        private readonly Random random;
        private readonly UnitTypeRegistry unitTypes = new UnitTypeRegistry();
        private readonly TechnologyTree technologyTree = new TechnologyTree();
        private bool isRunning = true;
        #endregion

        #region Constructors
        public Match(World world, Random random)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(random, "random");

            this.world = world;
            this.random = random;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a message was received from a <see cref="Faction"/>.
        /// </summary>
        public event Action<Match, FactionMessage> FactionMessageReceived;

        [Obsolete("To be handled in GameStates.")]
        public event Action<Match> Quitting;
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

        [Obsolete("To be handled in GameStates.")]
        public bool IsRunning
        {
            get { return isRunning; }
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
        [Obsolete("To be handled in GameStates.")]
        public void Pause()
        {
            isRunning = false;
        }

        [Obsolete("To be handled in GameStates.")]
        public void Resume()
        {
            isRunning = true;
        }

        /// <summary>
        /// Notifies this <see cref="Match"/> that a message from a <see cref="Faction"/> has been received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void PostFactionMessage(FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");
            FactionMessageReceived.Raise(this, message);
        }
        #endregion
    }
}
