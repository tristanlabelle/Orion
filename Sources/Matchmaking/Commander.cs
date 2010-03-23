using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Matchmaking.Commands.Pipeline;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Abstract base class for commanders, classes responsible of generating
    /// <see cref="Command"/>s that alter the game's state.
    /// </summary>
    public abstract class Commander
    {
        #region Fields
        private readonly Faction faction;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Commander"/> from the <see cref="Faction"/> it controls.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Commander"/> controls.</param>
        protected Commander(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            this.faction = faction;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Commander"/> generates a <see cref="Command"/>.
        /// </summary>
        public event Action<Commander, Command> CommandGenerated;

        private void OnCommandGenerated(Command command)
        {
            if (CommandGenerated != null) CommandGenerated(this, command);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Faction"/> that this <see cref="Commander"/> is in control of.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }

        /// <summary>
        /// Gets the <see cref="World"/> of the <see cref="Faction"/>
        /// which is controlled by this <see cref="Commander"/>.
        /// </summary>
        public World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Raises the <see cref="E:CommandGenerated"/> event
        /// with the <see cref="Command"/> that was generated.
        /// </summary>
        /// <param name="command">The <see cref="Command"/> that was generated.</param>
        protected void GenerateCommand(Command command)
        {
            Argument.EnsureNotNull(command, "command");

            Argument.EnsureNotNull(command, "command");
            if (Faction.Status == FactionStatus.Defeated)
            {
                Debug.Fail("A command was generated after the death of the faction.");
                return;
            }

            OnCommandGenerated(command);
        }

        /// <summary>
        /// Called by the pipeline to update this <see cref="Commander"/> for a frame, giving it a chance
        /// to flush its local pipeline.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame, in seconds.</param>
        public virtual void Update(float timeDelta) { }

        public override string ToString()
        {
            return "{0} of {1}".FormatInvariant(GetType().Name, faction);
        }
        #endregion
    }
}
