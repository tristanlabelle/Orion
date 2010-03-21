using System;
using System.Collections.Generic;
using System.Diagnostics;
using Orion.Engine;

namespace Orion.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// Base class for command filters which receive commands, operate on them and forward them to a sink.
    /// </summary>
    public abstract class CommandFilter : ICommandSink
    {
        #region Events
        /// <summary>
        /// Raised when an event has been flushed by this filter.
        /// </summary>
        public event Action<CommandFilter, Command> Flushed;
        #endregion

        #region Methods
        protected void Flush(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            if (Flushed != null) Flushed(this, command);
        }

        public abstract void Handle(Command command);
        public abstract void Update(int updateNumber, float timeDeltaInSeconds);
        public virtual void Dispose() { }
        #endregion
    }
}