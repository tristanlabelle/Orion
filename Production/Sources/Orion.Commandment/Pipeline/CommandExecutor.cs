using System;
using System.Collections.Generic;
using Orion.GameLogic;
using System.Diagnostics;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command sink which executes the <see cref="Command"/>s which reaches it.
    /// </summary>
    public sealed class CommandExecutor : ICommandSink
    {
        #region Fields
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly Match match;
        #endregion

        #region Constructors
        public CommandExecutor(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            this.match = match;
        }
        #endregion

        #region Methods
        public void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            commandQueue.Enqueue(command);
        }

        public void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (commandQueue.Count > 0)
            {
                Command command = commandQueue.Dequeue();

                if (command.ValidateHandles(match.World))
                {
                    Debug.WriteLine("Update #{0}: {1}.".FormatInvariant(updateNumber, command));
                    command.Execute(match);
                }
                else
                {
                    Debug.WriteLine("Update #{0}: Discarding {1}.".FormatInvariant(updateNumber, command));
                }
            }
        }
        #endregion

        #region Explicit Members
        void IDisposable.Dispose() { }
        #endregion
    }
}
