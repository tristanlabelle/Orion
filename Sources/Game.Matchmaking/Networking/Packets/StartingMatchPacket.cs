using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent to peers by the host to indicate that the game is starting,
    /// or that is broadcasted to indicate that the game should be removed from the lobby.
    /// </summary>
    public sealed class StartingMatchPacket : GamePacket
    {   
        #region Fields
        public static readonly StartingMatchPacket Instance = new StartingMatchPacket();
        #endregion

        #region Methods
        public static void Serialize(StartingMatchPacket packet, BinaryWriter writer) { }

        public static StartingMatchPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
