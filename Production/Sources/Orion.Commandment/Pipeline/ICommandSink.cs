using System;

namespace Orion.Commandment.Pipeline
{
    public interface ICommandSink
    {
        void Feed(Command command);
        void EndFeed();
    }
}
