using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent when a peer wants to join another peer's game.
    /// </summary>
    public sealed class JoinRequestPacket : GamePacket
    {
        #region Fields
        public static readonly JoinRequestPacket Instance = new JoinRequestPacket();
        #endregion

        #region Methods
        public static void Serialize(JoinRequestPacket packet, BinaryWriter writer) { }

        public static JoinRequestPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
