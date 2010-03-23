using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Game.Matchmaking.Networking
{
    public enum SetupMessageType : byte
    {
        JoinRequest = 101,
        AcceptJoinRequest = 102,
        RefuseJoinRequest = 103,
        LeaveGame = 104,

        Explore = 105,
        Advertise = 106,
        RemoveGame = 113,

        SetPeer = 107,
        SetSlot = 108,
        SetSeed = 109,

        Message = 110,
        ChangeSize = 111,

        StartGame = 112,
        Exit = 114
    }
}
