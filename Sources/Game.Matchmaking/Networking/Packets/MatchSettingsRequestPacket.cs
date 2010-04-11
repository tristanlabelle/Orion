using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet sent when a peer requests to received the updated settings of the match
    /// from the host.
    /// </summary>
    public sealed class MatchSettingsRequestPacket : GamePacket
    {
        #region Fields
        public static readonly MatchSettingsRequestPacket Instance = new MatchSettingsRequestPacket();
        #endregion

        #region Methods
        public static void Serialize(MatchSettingsRequestPacket packet, BinaryWriter writer) { }

        public static MatchSettingsRequestPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
