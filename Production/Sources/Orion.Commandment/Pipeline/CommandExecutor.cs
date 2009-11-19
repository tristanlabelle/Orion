using System;
using System.Collections.Generic;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command sink which executes the <see cref="Command"/>s which reaches it.
    /// </summary>
    public sealed class CommandExecutor : ICommandSink
    {
        #region Fields
        private readonly Queue<Command> commandQueue = new Queue<Command>();
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
                command.Execute();
            }
        }
        #endregion

        #region Explicit Members
        void IDisposable.Dispose() { }
        #endregion
    }
}
