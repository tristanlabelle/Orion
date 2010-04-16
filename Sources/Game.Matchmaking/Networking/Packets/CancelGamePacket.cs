using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    public class CancelGamePacket : GamePacket
    {
        #region Fields
        private static CancelGamePacket instance = new CancelGamePacket();
        #endregion

        #region Methods
        public static void Serialize(CancelGamePacket packet, BinaryWriter writer)
        { }

        public static CancelGamePacket Deserialize(BinaryReader reader)
        {
            return instance;
        }
        #endregion
    }
}
