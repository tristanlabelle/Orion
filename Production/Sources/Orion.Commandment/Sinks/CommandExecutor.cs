using System;

namespace Orion.Commandment
{
    public sealed class CommandExecutor : ICommandSink
    {
        #region Fields
        private int commandIndex = 0;
        #endregion

        #region Methods
        public void Feed(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            command.Execute();
        }
        #endregion

        #region Explicit Members
        void ICommandSink.BeginFeed() { }
        void ICommandSink.EndFeed() { }
        #endregion
    }
}
