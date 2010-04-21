using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Orion.Engine.Collections;
using Orion.Engine.Networking;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// A Transporter is responsible for safely transporting UDP packets over a network.
    /// It guarantees that packets are going to arrive without loss or duplication and as sent.
    /// The order of arrival is guaranteed.
    /// It creates a single UDP socket for communication to various hosts.
    /// The remote host must use a Transporter as well for reception.
    /// </summary>
    public sealed class SafeTransporter : IDisposable
    {
        #region Fields
        #region Constants
        private static readonly TimeSpan SafePacketTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DefaultPacketResendDelay = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan MinimumPacketResendDelay = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// The amount of time to block in Socket.ReceiveFrom calls.
        /// </summary>
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(2);
        #endregion

        private readonly List<PeerLink> peers = new List<PeerLink>();
        private readonly HashSet<IPv4EndPoint> timedOutPeerEndPoints = new HashSet<IPv4EndPoint>();

        /// <summary>
        /// The underlying socket. Accessed by both threads, synchronized by locking this object.
        /// </summary>
        private readonly Socket socket;

        /// <summary>
        /// The port on which the underlying socket is bound.
        /// </summary>
        private readonly ushort port;

        /// <summary>
        /// A collection of addresses assigned to this computer.
        /// </summary>
        private readonly ReadOnlyCollection<IPv4Address> localAddresses;

        /// <summary>
        /// The thread which sends and receives data. 
        /// </summary>
        /// <remarks>
        /// Accessed only from the main thread.
        /// </remarks>
        private readonly Thread workerThread;

        /// <remarks>
        /// Accessed only from the worker thread.
        /// </remarks>
        private readonly byte[] receptionBuffer = new byte[2048];
        /// <remarks>
        /// Accessed only from the worker thread.
        /// </remarks>
        private EndPoint senderEndPoint = new IPEndPoint(0, 0);

        /// <remarks>
        /// Read and written by the main thread, only read by the worker thread.
        /// Writing is synchronized by the socketMutex.
        /// </remarks>
        private volatile bool isDisposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Transporter whose UDP socket is bound to a specified port on all interfaces.
        /// </summary>
        /// <param name="port">
        /// The port on which to bind
        /// </param>
        public SafeTransporter(int port)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(new IPEndPoint(IPAddress.Any, port));
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            this.socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout, (int)ReceiveTimeout.TotalMilliseconds);
            this.socket.MulticastLoopback = false;

            this.port = (ushort)((IPEndPoint)socket.LocalEndPoint).Port;

            // Attempt to find the addresses of this computer to detect
            // broadcasts which come back to this computer and ignore them.
            try
            {
                string hostName = Dns.GetHostName();
                this.localAddresses = Dns.GetHostAddresses(hostName)
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(address => (IPv4Address)address)
                    .ToList()
                    .AsReadOnly();
            }
            catch (SocketException)
            {
                this.localAddresses = new ReadOnlyCollection<IPv4Address>(new IPv4Address[0]);
            }

            this.workerThread = new Thread(WorkerThreadEntryPoint);
            this.workerThread.Name = "SafeTransporter worker thread";
            this.workerThread.IsBackground = true;
            this.workerThread.Start();
        }

        /// <summary>
        /// Creates a new Transporter whose UDP socket is bound to a random port on all interfaces.
        /// </summary>
        public SafeTransporter()
            : this(0) { }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a packet arrives.
        /// </summary>
        /// <remarks>This event is only raised when the method <see cref="M:Poll"/> is called.</remarks>
        public event Action<SafeTransporter, NetworkEventArgs> Received;

        /// <summary>
        /// Raised when a packet cannot reach its destination.
        /// </summary>
        /// <remarks>This event is only raised when the method <see cref="M:Poll"/> is called.</remarks>
        public event Action<SafeTransporter, IPv4EndPoint> TimedOut;

        private void OnReceived(IPv4EndPoint hostEndPoint, byte[] message)
        {
            Action<SafeTransporter, NetworkEventArgs> handler = Received;
            if (handler != null) handler(this, new NetworkEventArgs(hostEndPoint, message));
        }

        private void OnTimedOut(IPv4EndPoint endPoint)
        {
            Action<SafeTransporter, IPv4EndPoint> handler = TimedOut;
            if (handler != null) handler(this, endPoint);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the port to which this transporter is locally bound.
        /// </summary>
        public ushort Port
        {
            get { return port; }
        }
        #endregion

        #region Methods
        #region Private Helpers
        private PeerLink GetOrCreatePeerLink(IPv4EndPoint endPoint)
        {
            PeerLink peer = peers.FirstOrDefault(p => p.EndPoint == endPoint);
            if (peer == null)
            {
                peer = new PeerLink(endPoint);
                peers.Add(peer);
            }

            return peer;
        }

        private bool IsSelf(IPv4EndPoint endPoint)
        {
            return localAddresses.Contains(endPoint.Address)
                && endPoint.Port == port;
        }
        #endregion

        #region Worker Thread
        private void WorkerThreadEntryPoint()
        {
            try
            {
                while (!isDisposed)
                {
                    UpdateReceiving();
                    if (isDisposed) break;

                    UpdateSending();
                    if (isDisposed) break;

                    Thread.Sleep(10);
                }
            }
            catch (SocketException exception)
            {
                Debug.Fail(
                    "Unexpected socket exception {0}: {1}. Interrupting all worker thread activities."
                    .FormatInvariant(exception.ErrorCode, exception));
            }
        }

        private void UpdateReceiving()
        {
            while (!isDisposed)
            {
                int availableDataLength = socket.Available;
                if (availableDataLength == 0) break;

                int packetLength = -1;
                try
                {
                    packetLength = socket.ReceiveFrom(receptionBuffer, ref senderEndPoint);
                    if (packetLength >= receptionBuffer.Length)
                    {
                        throw new ApplicationException(
                            "A {0}-bytes packet was received and truncated."
                            .FormatInvariant(packetLength));
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        Debug.Fail("Connection reset in Socket.ReceiveFrom. This might be non-critical.");
                        continue;
                    }

                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        Debug.Fail("Socket.ReceiveFrom should not time out, we made sure that it had data available.");
                        break;
                    }

                    throw;
                }

                if (packetLength == 0)
                {
                    Debug.Fail("Unexpected zero-length packet. Ignored.");
                    break;
                }

                IPv4EndPoint ipv4SenderEndPoint = (IPv4EndPoint)senderEndPoint;
                if (IsSelf(ipv4SenderEndPoint) && Protocol.GetPacketType(receptionBuffer) == PacketType.Broadcast)
                {
                    // Ignore packets we broadcasted ourself.
                    return;
                }

                HandlePacket(ipv4SenderEndPoint, receptionBuffer, packetLength);
            }
        }

        private void HandlePacket(IPv4EndPoint senderEndPoint, byte[] data, int length)
        {
            if (Protocol.IsForeign(data))
            {
                Debug.Fail("Foreign packet received.");
                return;
            }

            lock (peers)
            {
                PeerLink peer = GetOrCreatePeerLink(senderEndPoint);
                if (peer.HasTimedOut)
                {
                    Debug.Fail(
                        "Received a packet from timed out peer {0}."
                        .FormatInvariant(senderEndPoint));
                    return;
                }

                PacketType type = Protocol.GetPacketType(data);
                if (type == PacketType.Message)
                {
                    uint number = Protocol.GetDataPacketNumber(data);
                    if (!peer.HasReceivedMessage(number))
                    {
                        byte[] message = Protocol.GetDataPacketMessage(data, length);
                        peer.AddReceivedMessage(number, message);
                    }

                    SendAcknowledgement(senderEndPoint, number);
                }
                else if (type == PacketType.Acknowledgement)
                {
                    uint number = Protocol.GetAcknowledgementPacketNumber(data);
                    peer.MarkMessageAsAcknowledged(number);
                }
                else if (type == PacketType.Broadcast)
                {
                    byte[] message = Protocol.GetBroadcastPacketMessage(data, length);
                    peer.AddReceivedBroadcastMessage(message);
                }
                else if (type == PacketType.Ping)
                {
                    Debug.WriteLine("SafeTransporter ping'ed by {0}".FormatInvariant(senderEndPoint));
                }
                else
                {
                    Debug.Fail("Received packet with unknown type.");
                }
            }
        }

        private void SendAcknowledgement(IPv4EndPoint hostEndPoint, uint number)
        {
            byte[] acknowledgementPacketData = Protocol.CreateAcknowledgementPacket(number);
            socket.SendTo(acknowledgementPacketData, hostEndPoint);
        }
        #endregion

        #region Sender Thread
        private void UpdateSending()
        {
            lock (peers)
            {
                foreach (PeerLink peer in peers)
                {
                    if (peer.HasTimedOut) continue;

                    foreach (SafePacket packet in peer.PacketsToSend)
                    {
                        if (packet.TimeElapsedSinceCreation > SafePacketTimeout)
                        {
                            peer.MarkAsTimedOut();
                            timedOutPeerEndPoints.Add(peer.EndPoint);
                            break;
                        }

                        TimeSpan resendDelay = GetResendDelay(peer);
                        if (!packet.WasSent || packet.TimeElapsedSinceLastSend >= resendDelay)
                        {
                            socket.SendTo(packet.Data, peer.EndPoint);
                            packet.UpdateSendTime();
                        }
                    }
                }
            }
        }

        private TimeSpan GetResendDelay(PeerLink peer)
        {
            if (!peer.HasPingData) return DefaultPacketResendDelay;
            TimeSpan resendDelay = peer.AveragePing + peer.AveragePingDeviation;
            return resendDelay < MinimumPacketResendDelay ? MinimumPacketResendDelay : resendDelay;
        }
        #endregion

        #region Sending
        /// <summary>
        /// Sends a message to a specified host.
        /// </summary>
        /// <param name="message">
        /// The message to be sent.
        /// </param>
        /// <param name="remoteHost">
        /// The host to which the message is addressed.
        /// </param>
        public void SendTo(Subarray<byte> message, IPv4EndPoint hostEndPoint)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message.Array, "message.Array");
            Debug.Assert(message.Count < 512, "Warning: A network message exceeded 512 bytes.");
            Debug.Assert(!IsSelf(hostEndPoint), "Sending a packet to ourself.");

            lock (peers)
            {
                PeerLink peer = GetOrCreatePeerLink(hostEndPoint);
                peer.CreatePacket(message);
            }
        }

        public void SendTo(Subarray<byte> message, IEnumerable<IPv4EndPoint> hostEndPoints)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message.Array, "message.Array");
            Argument.EnsureNotNull(hostEndPoints, "hostEndPoints");

            foreach (IPv4EndPoint endPoint in hostEndPoints)
                SendTo(message, endPoint);
        }

        /// <summary>
        /// Broadcasts a message using to all listening safe transporters.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <param name="port">The port on which to broadcast</param>
        /// <remarks>
        /// Broadcasted packets are inherently unreliable as it is impossible to know who didn't receive them.
        /// Consequently, they are sent only once with only integrity being garanteed.
        /// </remarks>
        public void Broadcast(Subarray<byte> message, int port)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message.Array, "message.Array");
            Argument.EnsureWithin(port, 1, ushort.MaxValue, "port");
            Debug.Assert(message.Count < 512, "Warning: A network broadcast message exceeded 512 bytes.");

            IPv4EndPoint broadcastEndPoint = new IPv4EndPoint(IPv4Address.Broadcast, port);
            byte[] packetData = Protocol.CreateBroadcastPacket(message);

            socket.SendTo(packetData, broadcastEndPoint);
        }

        /// <summary>
        /// Pings a given end point. Used to open firewall ports.
        /// </summary>
        /// <param name="endPoint">The end point to be pinged.</param>
        public void Ping(IPv4EndPoint endPoint)
        {
            socket.SendTo(Protocol.CreatePingPacket(), endPoint);
        }
        #endregion

        #region Polling
        /// <summary>
        /// Triggers any pending packet reception event.
        /// </summary>
        /// <remarks>Events are not triggered until you call this method.</remarks>
        public void Poll()
        {
            EnsureNotDisposed();

            RaiseReceivedEvents();
            RaiseTimedOutEvents();
        }

        /// <summary>
        /// Triggers reception and time out events for a given host.
        /// </summary>
        /// <param name="endPoint">The end point of the host to be polled.</param>
        public void Poll(IPv4EndPoint endPoint)
        {
            EnsureNotDisposed();

            lock (peers)
            {
                if (timedOutPeerEndPoints.Remove(endPoint))
                {
                    OnTimedOut(endPoint);
                }
                else
                {
                    PeerLink peer = peers.FirstOrDefault(p => p.EndPoint == endPoint);
                    if (peer != null && peer.HasAvailableMessage)
                    {
                        OnReceived(peer.EndPoint, peer.PopNextAvailableMessage());
                    }
                }
            }
        }

        private void RaiseReceivedEvents()
        {
            lock (peers)
            {
                foreach (PeerLink peer in peers)
                {
                    while (peer.HasAvailableMessage)
                    {
                        OnReceived(peer.EndPoint, peer.PopNextAvailableMessage());
                    }
                }
            }
        }

        private void RaiseTimedOutEvents()
        {
            lock (peers)
            {
                foreach (IPv4EndPoint peerEndPoint in timedOutPeerEndPoints)
                    OnTimedOut(peerEndPoint);

                timedOutPeerEndPoints.Clear();
            }
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            EnsureNotDisposed();

            isDisposed = true;

            // Block until the worker threads terminates, with a timeout
            bool terminated = workerThread.Join(TimeSpan.FromSeconds(1));
            Debug.Assert(terminated, "The SafeTransporter worker thread termination has timed out.");

            // Kill the socket
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
        }

        public override string ToString()
        {
            return "SafeTransporter on port {0}".FormatInvariant(port);
        }
        #endregion
        #endregion
    }
}