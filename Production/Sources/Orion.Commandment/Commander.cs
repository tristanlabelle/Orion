using Orion.GameLogic;

namespace Orion.Commandment
{
    /// <summary>
    /// Abstract base class for commanders, classes responsible of generating
    /// <see cref="Command"/>s that alter the game's state.
    /// </summary>
    public abstract class Commander
    {
        #region Fields
        private readonly Faction faction;
        protected ICommandSink commandsEntryPoint;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Commander"/> from the <see cref="Faction"/> it controls.
        /// </summary>
        /// <param name="faction">The <see cref="Faction"/> this <see cref="Commander"/> controls.</param>
        public Commander(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            this.faction = faction;
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
        /// which  is controlled by this <see cref="Commander"/>.
        /// </summary>
        protected World World
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
            commandsEntryPoint.Feed(command);
        }

        /// <summary>
        /// Schedules the commander to be updated by a given pipeline on the run loop.
        /// </summary>
        /// <param name="pipeline">The pipeline on which to schedule this <see cref="Commander"/></param>
        public abstract void AddToPipeline(CommandPipeline pipeline);

        /// <summary>
        /// Called by the pipeline to update this <see cref="Commander"/> for a frame, giving it a chance
        /// to flush its local pipeline.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame, in seconds.</param>
        public virtual void Update(float timeDelta)
        { }

        public override string ToString()
        {
            return "{0} of {1}".FormatInvariant(GetType().Name, faction);
        }
        #endregion
    }
}
