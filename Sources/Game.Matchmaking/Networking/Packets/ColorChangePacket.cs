using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public sealed class ColorChangePacket : GamePacket
    {
        #region Fields
        private readonly int index;
        private readonly ColorRgb color;
        #endregion

        #region Constructors
        public ColorChangePacket(int index, ColorRgb color)
        {
            this.index = index;
            this.color = color;
        }
        #endregion

        #region Properties
        public int Index
        {
            get { return index; }
        }

        public ColorRgb Color
        {
            get { return color; }
        }
        #endregion

        #region Methods
        public static void Serialize(ColorChangePacket packet, BinaryWriter writer)
        {
            writer.Write(packet.index);
            packet.color.Serialize(writer);
        }

        public static ColorChangePacket Deserialize(BinaryReader reader)
        {
            return new ColorChangePacket(reader.ReadInt32(), ColorRgb.Deserialize(reader));
        }
        #endregion
    }
}
