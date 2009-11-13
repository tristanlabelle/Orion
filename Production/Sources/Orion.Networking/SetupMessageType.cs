using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Networking
{
    public enum SetupMessageType : byte
    {
        JoinRequest = 101,
        AcceptJoinRequest = 102,
        RefuseJoinRequest = 103,
        AddPeer = 104,
        KickPeer = 105,
        LeaveGame = 106,
        Seed = 107,
        Explore = 108,
        Advertise = 109,
        GameStarted = 110
    }
}
