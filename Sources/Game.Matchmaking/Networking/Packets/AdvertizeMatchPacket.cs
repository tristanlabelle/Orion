using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet which advertizes the existence of a match.
    /// </summary>
    public sealed class AdvertizeMatchPacket : GamePacket
    {
        #region Fields
        private readonly int openSlotCount;
        #endregion

        #region Constructors
        public AdvertizeMatchPacket(int openSlotCount)
        {
            Argument.EnsurePositive(openSlotCount, "openSlotCount");

            this.openSlotCount = openSlotCount;
        }
        #endregion

        #region Properties
        public int OpenSlotCount
        {
            get { return openSlotCount; }
        }
        #endregion

        #region Methods
        public static void Serialize(AdvertizeMatchPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(packet.openSlotCount);
        }

        public static AdvertizeMatchPacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            int openSlotCount = reader.ReadInt32();
            return new AdvertizeMatchPacket(openSlotCount);
        }
        #endregion
    }
}
