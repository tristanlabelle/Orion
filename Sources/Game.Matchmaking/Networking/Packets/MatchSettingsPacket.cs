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
        private readonly MatchSettings settings;
        #endregion

        #region Constructors
        public MatchSettingsPacket(MatchSettings matchSettings)
        {
            this.settings = matchSettings;
        }
        #endregion

        #region Properties
        public MatchSettings Settings
        {
            get { return settings; }
        }
        #endregion

        #region Methods
        public static void Serialize(MatchSettingsPacket packet, BinaryWriter writer)
        {
            packet.settings.Serialize(writer);
        }

        public static MatchSettingsPacket Deserialize(BinaryReader reader)
        {
            MatchSettings matchSettings = new MatchSettings();
            matchSettings.Deserialize(reader);
            return new MatchSettingsPacket(matchSettings);
        }
        #endregion
    }
}
