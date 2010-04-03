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
        private SimulationStep lastSimulationStep;
        private bool isPausable;
        private bool isRunning;
        #endregion

        #region Constructors
        public Match(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            world.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Match"/> gets updated for a frame.
        /// </summary>
        public event Action<Match, UpdateEventArgs> Updated;

        /// <summary>
        /// Raised when a message was received from a <see cref="Faction"/>.
        /// </summary>
        public event Action<Match, FactionMessage> FactionMessageReceived;

        /// <summary>
        /// Raised when allied factions defeat all their enemies.
        /// </summary>
        public event Action<Match, IEnumerable<Faction>> WorldConquered;

        public event Action<Match> Quitting;

        private void RaiseUpdated(float timeDeltaInSeconds)
        {
            var handler = Updated;
            if (handler != null) handler(this, new UpdateEventArgs(timeDeltaInSeconds));
        }

        private void RaiseFactionMessageReceived(FactionMessage message)
        {
            var handler = FactionMessageReceived;
            if (handler != null) handler(this, message);
        }

        private void RaiseWorldConquered(IEnumerable<Faction> faction)
        {
            var handler = WorldConquered;
            if (handler != null) handler(this, faction);
        }

        private void RaiseQuitting()
        {
            var handler = Quitting;
            if (handler != null) handler(this);
        }
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
        #endregion

        #region Methods
        public void Start()
        {
            isRunning = true;
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

            RaiseUpdated(timeDeltaInSeconds);
        }

        #region Faction Stuff
        private void OnFactionDefeated(World sender, Faction args)
        {
            CheckForWorldConquered();
        }

        /// <summary>
        /// Notifies this <see cref="Match"/> that a message from a <see cref="Faction"/> has been received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        public void PostFactionMessage(FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");
            RaiseFactionMessageReceived(message);
        }

        private void CheckForWorldConquered()
        {
            List<Faction> aliveFactions = world.Factions.Where(faction => faction.Status == FactionStatus.Undefeated).ToList();
            for (int i = 0; i < aliveFactions.Count; i++)
            {
                Faction referenceFaction = aliveFactions[i];
                for (int j = i + 1; j < aliveFactions.Count; j++)
                {
                    Faction checkedFaction = aliveFactions[j];
                    DiplomaticStance referenceDiplomaticStance = referenceFaction.GetDiplomaticStance(checkedFaction);
                    DiplomaticStance checkedDiplomaticStance = checkedFaction.GetDiplomaticStance(referenceFaction);
                    if (referenceDiplomaticStance != DiplomaticStance.Ally || checkedDiplomaticStance != DiplomaticStance.Ally)
                        return;
                }
            }
            RaiseWorldConquered(aliveFactions);
        }
        #endregion

        #region Pause/Resume/Quit
        public void Pause()
        {
            isRunning = false;
        }

        public void Resume()
        {
            isRunning = true;
        }

        public void Quit()
        {
            RaiseQuitting();
        }
        #endregion
        #endregion
    }
}
