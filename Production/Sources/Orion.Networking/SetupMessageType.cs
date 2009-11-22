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
        LeaveGame = 104,

        Explore = 105,
        Advertise = 106,

        SetPeer = 107,
        SetSlot = 108,
        SetSeed = 109,

        Message = 110,

        StartGame = 111,
        Exit = 112
    }
}
