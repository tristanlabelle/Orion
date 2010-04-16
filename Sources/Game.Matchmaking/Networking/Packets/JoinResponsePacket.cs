using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Networking.Packets
{
    /// <summary>
    /// A packet that is sent by the game host to accept or deny a join request.
    /// </summary>
    public sealed class JoinResponsePacket : GamePacket
    {
        #region Fields
        public static readonly JoinResponsePacket Accepted = new JoinResponsePacket(true);
        public static readonly JoinResponsePacket Refused = new JoinResponsePacket(false);

        private readonly bool wasAccepted;
        #endregion

        #region Constructors
        public JoinResponsePacket(bool wasAccepted)
        {
            this.wasAccepted = wasAccepted;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if the join request was accepted.
        /// </summary>
        public bool WasAccepted
        {
            get { return wasAccepted; }
        }
        #endregion

        #region Methods
        public static void Serialize(JoinResponsePacket packet, BinaryWriter writer)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(writer, "writer");

            writer.Write(packet.wasAccepted);
        }

        public static JoinResponsePacket Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            bool wasAccepted = reader.ReadBoolean();
            return wasAccepted ? Accepted : Refused;
        }
        #endregion
    }
}
