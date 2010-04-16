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
    public sealed class RemovePlayerPacket : GamePacket
    {
        #region Fields
        private int index;
        #endregion

        #region Constructor
        // used when the client asks the host to be removed
        public RemovePlayerPacket()
        { }

        // used when the host tells clients to remove someone
        public RemovePlayerPacket(int playerIndex)
        {
            this.index = playerIndex;
        }
        #endregion

        #region Properties
        public int PlayerIndex
        {
            get { return index; }
        }
        #endregion

        #region Methods
        public static void Serialize(RemovePlayerPacket packet, BinaryWriter writer)
        {
            writer.Write(packet.index);
        }

        public static RemovePlayerPacket Deserialize(BinaryReader reader)
        {
            return new RemovePlayerPacket(reader.ReadInt32());
        }
        #endregion
    }
}
