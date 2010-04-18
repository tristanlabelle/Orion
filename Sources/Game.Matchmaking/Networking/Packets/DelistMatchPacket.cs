using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is broadcasted to indicate that a match should be removed from the lobby.
    /// </summary>
    public sealed class DelistMatchPacket : GamePacket
    {
        #region Fields
        public static readonly DelistMatchPacket Instance = new DelistMatchPacket();
        #endregion

        #region Methods
        public static void Serialize(DelistMatchPacket packet, BinaryWriter writer)
        { }

        public static DelistMatchPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
