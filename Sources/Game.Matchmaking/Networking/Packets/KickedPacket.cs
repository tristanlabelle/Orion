using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet which is sent when a player gets kicked.
    /// </summary>
    public sealed class KickedPacket : GamePacket
    {
        #region Fields
        public static readonly KickedPacket Instance = new KickedPacket();
        #endregion

        #region Methods
        public static void Serialize(KickedPacket packet, BinaryWriter writer) { }

        public static KickedPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
