using System;
using System.Collections.Generic;

namespace Orion.Commandment
{
    public class CommandExecutor : ICommandSink
    {
        public void BeginFeed()
        { }

        public void EndFeed()
        { }

        public void Feed(Command command)
        {
            command.Execute();
        }
    }
}
