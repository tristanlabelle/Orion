using System;

namespace Orion.Commandment
{
    public interface ISinkRecipient
    {
        void BeginFeed();
        void Feed(Command command);
        void EndFeed();
    }
}
