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

            packetWriter = new BinaryWriter(packetMemoryStream);

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

            byte[] data = GetPacketData(packet);
            transporter.SendTo(data, target);
        }

        public void Send(GamePacket packet, IEnumerable<IPv4EndPoint> targets)
        {
            Argument.EnsureNotNull(packet, "packet");
            Argument.EnsureNotNull(targets, "targets");

            byte[] data = GetPacketData(packet);
            transporter.SendTo(data, targets);
        }

        public void Broadcast(GamePacket packet)
        {
            Argument.EnsureNotNull(packet, "packet");

            byte[] data = GetPacketData(packet);
            transporter.Broadcast(data, preferredPort);
        }

        public void Dispose()
        {
            transporter.Dispose();
        }

        private byte[] GetPacketData(GamePacket packet)
        {
            packetMemoryStream.SetLength(0);
            packetMemoryStream.Position = 0;
            GamePacket.Serializer.Serialize(packet, packetWriter);

            return packetMemoryStream.ToArray();
        }

        private void OnPacketReceived(SafeTransporter sender, NetworkEventArgs args)
        {
            if (PacketReceived == null) return;

            var stream = new MemoryStream(args.Data, 1, args.Data.Length - 1);
            var reader = new BinaryReader(stream);
            GamePacket packet = GamePacket.Serializer.Deserialize(reader);

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
