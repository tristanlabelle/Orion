using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent to request peers to update their displayed match settings.
    /// </summary>
    public sealed class UpdateMatchSettingsPacket : GamePacket
    {
        #region Fields
        private readonly MatchSettings settings;
        #endregion

        #region Constructors
        public UpdateMatchSettingsPacket(MatchSettings settings)
        {
            Argument.EnsureNotNull(settings, "settings");

            this.settings = settings;
        }
        #endregion

        #region Properties
        public MatchSettings Settings
        {
            get { return settings; }
        }
        #endregion

        #region Methods
        public static void Serialize(UpdateMatchSettingsPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            packet.settings.Serialize(writer);
        }

        public static UpdateMatchSettingsPacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            MatchSettings settings = new MatchSettings();
            settings.Deserialize(reader);
            return new UpdateMatchSettingsPacket(settings);
        }
        #endregion
    }
}
