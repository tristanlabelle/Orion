using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A player representing the human in front of the local machine.
    /// </summary>
    public sealed class LocalPlayer : Player
    {
        #region Constructors
        public LocalPlayer(string name, ColorRgb color)
            : base(name, color) { }

        public LocalPlayer(ColorRgb color)
            : this(Environment.MachineName, color) { }
        #endregion

        #region Serialization
        public static void Serialize(LocalPlayer player, BinaryWriter writer)
        {
            Argument.EnsureNotNull(player, "player");
            Argument.EnsureNotNull(writer, "writer");

            SerializeNameAndColor(player, writer);
        }

        public static LocalPlayer Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            string name = reader.ReadString();
            ColorRgb color = ColorRgb.Deserialize(reader);
            return new LocalPlayer(name, color);
        }
        #endregion
    }
}
