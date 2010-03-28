using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Game.Matchmaking.Networking
{
    public enum SetupMessageType : byte
    {
        JoinRequest,
        AcceptJoinRequest,
        RefuseJoinRequest,
        LeaveGame,

        Explore,
        Advertise,
        RemoveGame,

        SetPeer,
        SetSlot,

        Message,
        ChangeOptions,

        StartGame,
        Exit
    }
}
