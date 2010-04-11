using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using System.IO;
using System.Collections.ObjectModel;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent to request peers to update their displayed players.
    /// </summary>
    public sealed class UpdatePlayersPacket : GamePacket
    {
        #region Fields
        private readonly ReadOnlyCollection<Player> nonHostPlayers;
        #endregion

        #region Constructors
        public UpdatePlayersPacket(IEnumerable<Player> nonHostPlayers)
        {
            Argument.EnsureNotNull(nonHostPlayers, "players");

            this.nonHostPlayers = nonHostPlayers.ToList().AsReadOnly();
            int localPlayerCount = this.nonHostPlayers.Count(p => p is LocalPlayer);
            if (localPlayerCount != 1)
            {
                throw new ArgumentException(
                    "The UpdatePlayersPacket should contain one and only one local player.",
                    "nonHostPlayers");
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of players in the game, excluding the host.
        /// </summary>
        public ReadOnlyCollection<Player> NonHostPlayers
        {
            get { return nonHostPlayers; }
        }
        #endregion

        #region Methods
        public static void Serialize(UpdatePlayersPacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            foreach (Player player in packet.nonHostPlayers)
                Player.Serializer.Serialize(player, writer);
        }

        public static UpdatePlayersPacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            List<Player> players = new List<Player>();

            var stream = reader.BaseStream;
            while (stream.Position < stream.Length)
            {
                Player player = Player.Serializer.Deserialize(reader);
                players.Add(player);
            }

            return new UpdatePlayersPacket(players);
        }
        #endregion
    }
}
