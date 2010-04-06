using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking
{
    public enum PlayerSlotType : byte
    {
        Closed = 0x00,
        Open = 0x10,
        AI = 0x20,
        Local = 0x30
    }
}
