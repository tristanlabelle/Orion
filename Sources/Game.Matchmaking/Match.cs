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
        private SimulationStep lastSimulationStep;
        private bool isPausable;
        private bool isRunning;
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
        /// Raised when this <see cref="Match"/> gets updated for a frame.
        /// </summary>
        [Obsolete("Only the command pipeline uses this, and it should be updated by the GameStates.")]
        public event Action<Match, UpdateEventArgs> Updated;

        /// <summary>
        /// Raised when a message was received from a <see cref="Faction"/>.
        /// </summary>
        public event Action<Match, FactionMessage> FactionMessageReceived;

        [Obsolete("To be handled in GameStates.")]
        public event Action<Match> Quitting;
        #endregion

        #region Properties
        public World World
        {
            get { return world; }
        }

        public bool IsPausable
        {
            get { return isPausable; }
            set { isPausable = value; }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

        /// <summary>
        /// Gets the number of the last simulation step that was run.
        /// </summary>
        public int LastSimulationStepNumber
        {
            get { return lastSimulationStep.Number; }
        }

        /// <summary>
        /// Gets the information associated with the last step of the game simulation.
        /// </summary>
        public SimulationStep LastSimulationStep
        {
            get { return lastSimulationStep; }
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
        public void Start()
        {
            isRunning = true;
        }

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

        [Obsolete("To be handled in GameStates.")]
        public void Quit()
        {
            Quitting.Raise(this);
        }

        /// <summary>
        /// Updates this <see cref="Match"/> for the duration of a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed, in seconds, since the last frame.</param>
        public void Update(float timeDeltaInSeconds)
        {
            if (IsRunning)
            {
                lastSimulationStep = new SimulationStep(
                    lastSimulationStep.Number + 1,
                    lastSimulationStep.TimeInSeconds + timeDeltaInSeconds,
                    timeDeltaInSeconds);

                world.Update(lastSimulationStep);
            }

            Updated.Raise(this, new UpdateEventArgs(timeDeltaInSeconds));
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
