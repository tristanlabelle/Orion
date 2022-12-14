using System;
using System.Collections.Generic;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands.Pipeline
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

        public void Update(SimulationStep step)
        {
            while (commandQueue.Count > 0)
            {
                Command command = commandQueue.Dequeue();

                if (command.ValidateHandles(match))
                {
                    command.Execute(match);
                }
                else
                {
#if DEBUG
                    // #if'd so FormatInvariant is not executed in release
                    Debug.WriteLine("Update #{0}: Discarding {1}.".FormatInvariant(step.Number, command));
#endif
                }
            }
        }
        #endregion

        #region Explicit Members
        void IDisposable.Dispose() { }
        #endregion
    }
}
