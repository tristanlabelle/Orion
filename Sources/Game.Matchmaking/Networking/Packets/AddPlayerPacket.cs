using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public class AddPlayerPacket : GamePacket
    {
        #region Fields
        private Player player;
        #endregion

        #region Constructor
        public AddPlayerPacket(Player player)
        {
            this.player = player;
        }
        #endregion

        #region Properties
        public Player Player
        {
            get { return player; }
        }
        #endregion

        #region Methods
        public static void Serialize(AddPlayerPacket packet, BinaryWriter writer)
        {
            Player.Serializer.Serialize(packet.player, writer);
        }

        public static AddPlayerPacket Deserialize(BinaryReader reader)
        {
            return new AddPlayerPacket(Player.Serializer.Deserialize(reader));
        }
        #endregion
    }
}
