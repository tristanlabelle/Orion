using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Game.Matchmaking.Networking
{
    public enum SetupMessageType : byte
    {
        JoinRequest = 12,
        AcceptJoinRequest,
        RefuseJoinRequest,
        LeaveGame,

        Explore,
        Advertise,
        RemoveGame,

        /// <summary>
        /// Queries the player slots and match settings.
        /// </summary>
        GetSetup,
        SetPeer,
        SetSlot,

        Message,
        ChangeOptions,

        StartGame,
        Exit
    }
}
