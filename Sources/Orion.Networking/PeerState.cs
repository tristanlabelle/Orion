using System;

namespace Orion.Networking
{
    [Flags]
    internal enum PeerState
    {
        None = 0,
        ReceivedCommands = 1,
        ReceivedDone = 2
    }
}
