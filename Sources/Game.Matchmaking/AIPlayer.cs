using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using System.IO;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A player which is controlled by an artificial intelligence.
    /// </summary>
    public sealed class AIPlayer : Player
    {
        #region Constructors
        public AIPlayer(string name, ColorRgb color)
            : base(name, color)
        { }
        #endregion

        #region Serialization
        public static void Serialize(AIPlayer player, BinaryWriter writer)
        {
            Argument.EnsureNotNull(player, "player");
            Argument.EnsureNotNull(writer, "writer");

            SerializeNameAndColor(player, writer);
        }

        public static AIPlayer Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            string name = reader.ReadString();
            ColorRgb color = DeserializeColor(reader);
            return new AIPlayer(name, color);
        }
        #endregion
    }
}
