using System;

namespace Orion.Game.Matchmaking.Networking
{
    public enum GameMessageType : byte
    {
        Commands = 0xC0,
        Done = 0xD0,
        Quit = 0xE0
    }
}