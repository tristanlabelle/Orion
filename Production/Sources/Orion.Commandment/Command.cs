using System;
using System.Collections.Generic;

using Orion.GameLogic;

namespace Orion.Commandment
{
    /// <summary>
    /// Abstract base class for commands, the atomic unit of game state change
    /// which encapsulate an order given by a <see cref="Commander"/>.
    /// </summary>
    public abstract class Command
    {
        #region Fields
        private readonly Handle factionHandle;
        #endregion

        #region Constructors
        protected Command(Handle factionHandle)
        {
            this.factionHandle = factionHandle;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the handle of the faction which created this command.
        /// </summary>
        public Handle FactionHandle
        {
            get { return factionHandle; }
        }

        /// <summary>
        /// Gets a sequence of handles to <see cref="Entity">entities</see> executing in this command.
        /// </summary>
        public abstract IEnumerable<Handle> ExecutingEntityHandles { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="world">The <see cref="World"/> in which the command should be executed.</param>
        public abstract void Execute(World world);

        public abstract override string ToString();
        #endregion
    }
}
