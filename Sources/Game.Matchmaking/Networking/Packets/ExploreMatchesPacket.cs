using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is broadcasted to invite hosts to advertize their matches.
    /// </summary>
    public sealed class ExploreMatchesPacket : GamePacket
    {   
        #region Fields
        public static readonly ExploreMatchesPacket Instance = new ExploreMatchesPacket();
        #endregion

        #region Methods
        public static void Serialize(ExploreMatchesPacket packet, BinaryWriter writer) { }

        public static ExploreMatchesPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
    }
}
