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

        /// <summary>
        /// Winsock error raised if the socket didn't receive anything before it timed out.
        /// </summary>
        private const int WSAETIMEDOUT = 10060;
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
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout, (int)ReceiveTimeout.TotalMilliseconds);
            socket.MulticastLoopback = false;

            port = ((IPEndPoint)socket.LocalEndPoint).Port;

            // Attempt to find the addresses of this computer to detect
            // broadcasts which come back to this computer and ignore them.
            try
            {
                string hostName = Dns.GetHostName();
                localAddresses = Dns.GetHostAddresses(hostName)
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(address => (IPv4Address)address)
                    .ToList()
                    .AsReadOnly();
            }
            catch (SocketException)
            {
                localAddresses = new ReadOnlyCollection<IPv4Address>(new IPv4Address[0]);
            }

            workerThread = new Thread(WorkerThreadEntryPoint);
            workerThread.Name = "SafeTransporter worker thread";
            workerThread.IsBackground = true;
            workerThread.Start();
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
        #region Peer Link Management
        private PeerLink GetOrCreatePeerLink(IPv4EndPoint ipEndPoint)
        {
            PeerLink peer = peers.FirstOrDefault(p => p.EndPoint == ipEndPoint);
            if (peer == null)
            {
                peer = new PeerLink(ipEndPoint);
                peers.Add(peer);
            }

            return peer;
        }
        #endregion

        #region Worker Thread
        private void WorkerThreadEntryPoint()
        {
            try
            {
                while (true)
                {
                    if (isDisposed) break;
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
            // The socket is locked once even if multiple packets are te be received.
            // This was chosen over locking while receiving and unlocking while handling
            // as multiple locking is supposed to be bad performance-wise.
            lock (socket)
            {
                if (isDisposed) return;

                while (true)
                {
                    int availableDataLength = socket.Available;
                    if (availableDataLength == 0) break;

                    Debug.Assert(availableDataLength <= receptionBuffer.Length,
                        "Available data exceeds the length of the reception buffer. The packet data could be truncated.");

                    try
                    {
                        int packetLength = socket.ReceiveFrom(receptionBuffer, ref senderEndPoint);
                        if (packetLength == 0)
                        {
                            Debug.Fail("Unexpected zero-length packet. Ignored.");
                            break;
                        }

                        IPv4EndPoint ipv4SenderEndPoint = (IPv4EndPoint)senderEndPoint;
                        if (localAddresses.Contains(ipv4SenderEndPoint.Address) && ipv4SenderEndPoint.Port == Port
                            && Protocol.GetPacketType(receptionBuffer) == PacketType.Broadcast)
                        {
                            // Ignore packets we broadcasted ourself.
                            return;
                        }

                        HandlePacket(ipv4SenderEndPoint, receptionBuffer, packetLength);
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode == WSAETIMEDOUT)
                        {
                            Debug.Fail("Socket.ReceiveFrom should not time out, we made sure that it had data available.");
                            break;
                        }
                        throw;
                    }
                }
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

            lock (socket)
            {
                if (isDisposed) return;
                socket.SendTo(acknowledgementPacketData, hostEndPoint);
            }
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
                            lock (socket)
                            {
                                if (isDisposed) return;
                                socket.SendTo(packet.Data, peer.EndPoint);
                            }

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
            Debug.Assert(!(localAddresses.Contains(hostEndPoint.Address) && hostEndPoint.Port == Port), "Sending a packet to ourself.");

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

            lock (socket)
            {
                if (isDisposed) return;
                socket.SendTo(packetData, broadcastEndPoint);
            }
        }

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

            lock (socket)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(null);
        }

        public override string ToString()
        {
            return string.Format("{{Transporter:{0}}}", Port);
        }
        #endregion
        #endregion
    }
}