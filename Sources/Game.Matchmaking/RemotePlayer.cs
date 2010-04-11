using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A player representing a remote network peer.
    /// </summary>
    public sealed class RemotePlayer : Player
    {
        #region Fields
        private readonly IPv4EndPoint endPoint;
        #endregion

        #region Constructors
        public RemotePlayer(IPv4EndPoint endPoint, string name, ColorRgb color)
            : base(name, color)
        {
            this.endPoint = endPoint;
        }

        [Obsolete("A name should be provided instead of using DNS resolution.")]
        public RemotePlayer(IPv4EndPoint endPoint, ColorRgb color)
            : this(endPoint, Dns.GetHostEntry(endPoint.Address).HostName, color)
        { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the IP endpoint of the remote player.
        /// </summary>
        public IPv4EndPoint EndPoint
        {
            get { return endPoint; }
        }
        #endregion

        #region Serialization
        public static void Serialize(RemotePlayer player, BinaryWriter writer)
        {
            Argument.EnsureNotNull(player, "player");
            Argument.EnsureNotNull(writer, "writer");

            SerializeNameAndColor(player, writer);
            writer.Write(player.endPoint.Address.Value);
            writer.Write(player.endPoint.Port);
        }

        public static RemotePlayer Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            string name = reader.ReadString();
            ColorRgb color = DeserializeColor(reader);
            IPv4EndPoint endPoint = new IPv4EndPoint( reader.ReadUInt32(), reader.ReadUInt16());

            return new RemotePlayer(endPoint, name, color);
        }
        #endregion
    }
}
