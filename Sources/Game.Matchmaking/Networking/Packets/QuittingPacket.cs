using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet which indicates that a peer is quitting.
    /// This is used both when a player exits while in the match configuration or in the actual game.
    /// </summary>
    public sealed class QuittingPacket : GamePacket
    {
        #region Fields
        public static readonly QuittingPacket Instance = new QuittingPacket();
        #endregion

        #region Methods
        public static void Serialize(QuittingPacket packet, BinaryWriter writer) { }

        public static QuittingPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
