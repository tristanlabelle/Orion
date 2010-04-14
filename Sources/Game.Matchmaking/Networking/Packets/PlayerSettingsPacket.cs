using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public sealed class PlayerSettingsPacket : GamePacket
    {
        #region Fields
        private readonly PlayerSettings settings;
        private readonly int playerIndex;
        #endregion

        #region Constructors
        public PlayerSettingsPacket(PlayerSettings settings, int playerIndex)
        {
            this.settings = settings;
            this.playerIndex = playerIndex;
        }
        #endregion

        #region Properties
        public int RecipientIndex
        {
            get { return playerIndex; }
        }

        public PlayerSettings Settings
        {
            get { return settings; }
        }
        #endregion

        #region Methods
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(playerIndex);
            settings.Serialize(writer);
        }

        public static PlayerSettingsPacket Deserialize(BinaryReader reader)
        {
            int playerIndex = reader.ReadInt32();
            PlayerSettings settings = new PlayerSettings();
            settings.Deserialize(reader);
            return new PlayerSettingsPacket(settings, playerIndex);
        }
        #endregion
    }
}
