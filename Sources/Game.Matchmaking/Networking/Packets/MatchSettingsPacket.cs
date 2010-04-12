using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public class MatchSettingsPacket : GamePacket
    {
        #region Fields
        private readonly PlayerSettings playerSettings;
        private readonly MatchSettings matchSettings;
        private readonly int recipientPlayerIndex;
        #endregion

        #region Constructors
        public MatchSettingsPacket(PlayerSettings playerSettings, MatchSettings matchSettings, int recipientPlayerIndex)
        {
            this.playerSettings = playerSettings;
            this.matchSettings = matchSettings;
            this.recipientPlayerIndex = recipientPlayerIndex;
        }
        #endregion

        #region Properties
        public PlayerSettings PlayerSettings
        {
            get { return playerSettings; }
        }
        #endregion

        #region Methods
        public static void Serialize(MatchSettingsPacket packet, BinaryWriter writer)
        {
            writer.Write(packet.recipientPlayerIndex);
            packet.playerSettings.Serialize(writer);
            packet.matchSettings.Serialize(writer);
        }

        public static MatchSettingsPacket Deserialize(BinaryReader reader)
        {
            int playerIndex = reader.ReadInt32();
            PlayerSettings playerSettings = new PlayerSettings();
            playerSettings.Deserialize(reader);
            MatchSettings matchSettings = new MatchSettings();
            matchSettings.Deserialize(reader);
            return new MatchSettingsPacket(playerSettings, matchSettings, playerIndex);
        }
        #endregion
    }
}
