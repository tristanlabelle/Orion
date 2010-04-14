using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public sealed class ColorChangeRequestPacket : GamePacket
    {
        #region Instance
        #region Fields
        public readonly ColorRgb Color;
        #endregion

        #region Constructors
        public ColorChangeRequestPacket(ColorRgb color)
        {
            Color = color;
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static void Serialize(ColorChangeRequestPacket packet, BinaryWriter writer)
        {
            packet.Color.Serialize(writer);
        }

        public static ColorChangeRequestPacket Deserialize(BinaryReader reader)
        {
            return new ColorChangeRequestPacket(ColorRgb.Deserialize(reader));
        }
        #endregion
        #endregion
    }
}
