using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Provides game-specific networking services.
    /// </summary>
    public sealed class GameNetworking : IDisposable
    {
        #region Fields
        private const int preferredPort = 41223;

        private readonly SafeTransporter transporter;
        private readonly MemoryStream packetMemoryStream = new MemoryStream(1024);
        private readonly BinaryWriter packetWriter;
        #endregion

        #region Constructors
        public GameNetworking()
        {
            try { transporter = new SafeTransporter(preferredPort); }
            catch (SocketException) { transporter = new SafeTransporter(); }

            packetWriter = new BinaryWriter(packetMemoryStream, Encoding.UTF8);

            transporter.Received += OnPacketReceived;
            transporter.TimedOut += OnPeerTimedOut;
        }
        #endregion

        #region Events
        public event Action<GameNetworking, GamePacketEventArgs> PacketReceived;
        public event Action<GameNetworking, IPv4EndPoint> PeerTimedOut;
        #endregion

        #region Properties
        public int PortNumber
        {
            get { return transporter.Port; }
        }
        #endregion

        #region Methods
        public void Send(GamePacket packet, IPv4EndPoint target)
        {
            Argument.EnsureNotNull(packet, "packet");

            ArraySegment<byte> data = GetPacketData(packet);
            transporter.SendTo(data, target);
        }

        public void Send(GamePacket packet, IEnumerable<IPv4EndPoint> targets)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(targets, "targets");

            ArraySegment<byte> data = GetPacketData(packet);
            transporter.SendTo(data, targets);
        }

        public void Broadcast(GamePacket packet)
        {
            Argument.EnsureNotNull(packet, "packet");

            ArraySegment<byte> data = GetPacketData(packet);
            transporter.Broadcast(data, preferredPort);
        }

        public void Ping(IPv4EndPoint endPoint)
        {
            transporter.Ping(endPoint);
        }

        public void Poll()
        {
            transporter.Poll();
        }

        public void Dispose()
        {
            transporter.Dispose();
        }

        private ArraySegment<byte> GetPacketData(GamePacket packet)
        {
            packetMemoryStream.SetLength(0);
            packetMemoryStream.Position = 0;
            GamePacket.Serializer.Serialize(packet, packetWriter);
            packetWriter.Flush();
            return new ArraySegment<byte>(packetMemoryStream.ToArray(), 0, (int)packetMemoryStream.Length);
        }

        private void OnPacketReceived(SafeTransporter sender, NetworkEventArgs args)
        {
            if (PacketReceived == null) return;

            GamePacket packet = GamePacket.Serializer.Deserialize(args.Data);

            GamePacketEventArgs eventArgs = new GamePacketEventArgs(args.Host, packet);
            PacketReceived(this, eventArgs);
        }

        private void OnPeerTimedOut(SafeTransporter sender, IPv4EndPoint peerEndPoint)
        {
            PeerTimedOut.Raise(this, peerEndPoint);
        }
        #endregion
    }
}
