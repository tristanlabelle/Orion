using System;
using System.IO;

namespace Orion.Game.Matchmaking.Networking.Packets
{
	/// <summary>
	/// A packet sent when a match host cancels the match.
	/// </summary>
	public sealed class CancelMatchPacket : GamePacket
	{
        #region Fields
        public static readonly CancelMatchPacket Instance = new CancelMatchPacket();
        #endregion

        #region Methods
        public static void Serialize(CancelMatchPacket packet, BinaryWriter writer) { }

        public static CancelMatchPacket Deserialize(BinaryReader reader)
        {
            return Instance;
        }
        #endregion
	}
}
