using System;

namespace Orion.Commandment
{
    public interface ICommandSink
    {
        void BeginFeed();
        void Feed(Command command);
        void EndFeed();
    }
}
