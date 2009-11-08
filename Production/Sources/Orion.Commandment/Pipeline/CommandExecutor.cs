using System;

namespace Orion.Commandment.Pipeline
{
    public sealed class CommandExecutor : ICommandSink
    {
        #region Methods
        public void Feed(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            command.Execute();
        }
        #endregion

        #region Explicit Members
        void ICommandSink.EndFeed() { }
        #endregion
    }
}
