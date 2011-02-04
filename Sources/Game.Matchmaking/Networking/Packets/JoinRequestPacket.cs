using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Engine;
using System.ComponentModel;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent when a peer wants to join another peer's game.
    /// </summary>
    [ImmutableObject(true)]
    public sealed class JoinRequestPacket : GamePacket
    {
        #region Fields
        private readonly string playerName;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="JoinRequestPacket"/> from the name of the joining player.
        /// </summary>
        /// <param name="playerName">The name of the joining player.</param>
        public JoinRequestPacket(string playerName)
        {
            Argument.EnsureNotNull(playerName, "playerName");

            this.playerName = playerName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the joining player.
        /// </summary>
        public string PlayerName
        {
            get { return playerName; }
        }
        #endregion

        #region Methods
        public static void Serialize(JoinRequestPacket packet, BinaryWriter writer)
        {
            writer.Write(packet.PlayerName);
        }

        public static JoinRequestPacket Deserialize(BinaryReader reader)
        {
            return new JoinRequestPacket(reader.ReadString());
        }
        #endregion
    }
}
