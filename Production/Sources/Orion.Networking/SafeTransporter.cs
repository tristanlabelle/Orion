using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Orion.Networking
{
    /// <summary>
    /// A Transporter is responsible for safely transporting UDP packets over a network.
    /// It guarantees that packets are going to arrive without loss or duplication and as sent.
    /// The order of arrival is not garanteed.
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
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Winsock error raised if the socket didn't receive anything before it timed out.
        /// </summary>
        private const int WSAETIMEDOUT = 10060;
        #endregion

        private readonly List<PeerLink> peers = new List<PeerLink>();
        private readonly HashSet<IPv4EndPoint> timedOutPeerEndPoints = new HashSet<IPv4EndPoint>();

        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Semaphore socketSemaphore = new Semaphore(2, 2);

        private readonly Thread senderThread;
        private readonly Thread receiverThread;

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
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            socket.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout, (int)ReceiveTimeout.TotalMilliseconds);
            
            senderThread = new Thread(SenderThreadEntryPoint);
            receiverThread = new Thread(ReceiverThreadEntryPoint);
            senderThread.Name = "Sender Thread for {0}".FormatInvariant(this);
            receiverThread.Name = "Receiver Thread for {0}".FormatInvariant(this);
            senderThread.IsBackground = true;
            receiverThread.IsBackground = true;
            senderThread.Start();
            receiverThread.Start();
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when a packet arrives.
        /// </summary>
        /// <remarks>This event is only raised when the method <see cref="M:Poll"/> is called.</remarks>
        public event GenericEventHandler<SafeTransporter, NetworkEventArgs> Received;

        /// <summary>
        /// Raised when a packet cannot reach its destination.
        /// </summary>
        /// <remarks>This event is only raised when the method <see cref="M:Poll"/> is called.</remarks>
        public event GenericEventHandler<SafeTransporter, IPv4EndPoint> TimedOut;

        private void OnReceived(IPv4EndPoint hostEndPoint, byte[] message)
        {
            GenericEventHandler<SafeTransporter, NetworkEventArgs> handler = Received;
            if (handler != null) handler(this, new NetworkEventArgs(hostEndPoint, message));
        }

        private void OnTimedOut(IPv4EndPoint endPoint)
        {
            GenericEventHandler<SafeTransporter, IPv4EndPoint> handler = TimedOut;
            if (handler != null) handler(this, endPoint);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the port to which this transporter is locally bound.
        /// </summary>
        public int Port
        {
            get { return ((IPEndPoint)socket.LocalEndPoint).Port; }
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

        #region Receiver Thread
        private void ReceiverThreadEntryPoint()
        {
            byte[] packetData = new byte[1024];

            while (!isDisposed)
            {
                try
                {
                    IPv4EndPoint senderEndPoint;
                    int packetLength = WaitForPacket(packetData, out senderEndPoint);
                    if (isDisposed) break;
                    HandlePacket(senderEndPoint, packetData, packetLength);
                }
                catch (SocketException exception)
                {
                    Debug.Fail(
                        "Unexpected socket exception {0}: {1}"
                        .FormatInvariant(exception.ErrorCode, exception));
                    break;
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
                else
                {
                    Debug.Fail("Received packet with unknown type.");
                }
            }
        }

        private int WaitForPacket(byte[] data, out IPv4EndPoint hostEndPoint)
        {
            EndPoint endPoint = new IPEndPoint(0, 0);
            hostEndPoint = IPv4EndPoint.Any;

            while (!isDisposed)
            {
                socketSemaphore.WaitOne();
                try
                {
                    if (isDisposed) break;
                    int packetLength = socket.ReceiveFrom(data, ref endPoint);
                    hostEndPoint = (IPv4EndPoint)(IPEndPoint)endPoint;
                    return packetLength;
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != WSAETIMEDOUT) throw;
                }
                finally
                {
                    socketSemaphore.Release();
                }
            }

            return -1;
        }

        private void SendAcknowledgement(IPv4EndPoint hostEndPoint, uint number)
        {
            byte[] acknowledgementPacketData = Protocol.CreateAcknowledgementPacket(number);

            socketSemaphore.WaitOne();
            try
            {
                if (isDisposed) return;
                socket.SendTo(acknowledgementPacketData, hostEndPoint);
            }
            finally
            {
                socketSemaphore.Release();
            }
        }
        #endregion

        #region Sender Thread
        private void SenderThreadEntryPoint()
        {
            while (!isDisposed)
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
                                if (packet.WasSent)
                                {
                                    Console.WriteLine(
                                        "Message #{0} resent after {1}."
                                        .FormatInvariant(packet.Number,
                                        packet.TimeElapsedSinceLastSend.Value));
                                }

                                socketSemaphore.WaitOne();
                                try
                                {
                                    if (isDisposed) return;
                                    socket.SendTo(packet.Data, peer.EndPoint);
                                }
                                finally
                                {
                                    socketSemaphore.Release();
                                }
                                packet.UpdateSendTime();
                            }
                        }
                    }
                }

                Thread.Sleep(0);
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
        public void SendTo(byte[] message, IPv4EndPoint hostEndPoint)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message, "message");
            Argument.EnsureNotNull(hostEndPoint, "hostEndPoint");
            Debug.Assert(message.Length < 512, "Warning: A network message exceeded 512 bytes.");

            lock (peers)
            {
                PeerLink peer = GetOrCreatePeerLink(hostEndPoint);
                peer.CreatePacket(message);
            }
        }

        public void SendTo(byte[] message, IEnumerable<IPv4EndPoint> hostEndPoints)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message, "message");
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
        public void Broadcast(byte[] message, int port)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(message, "message");
            Argument.EnsureWithin(port, 1, ushort.MaxValue, "port");
            Debug.Assert(message.Length < 512, "Warning: A network broadcast message exceeded 512 bytes.");

            IPv4EndPoint broadcastEndPoint = new IPv4EndPoint(IPv4Address.Broadcast, port);
            byte[] packetData = Protocol.CreateBroadcastPacket(message);

            socketSemaphore.WaitOne();
            try
            {
                if (isDisposed) return;
                socket.SendTo(packetData, broadcastEndPoint);
            }
            finally
            {
                socketSemaphore.Release();
            }
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

            socketSemaphore.WaitOne();
            socketSemaphore.WaitOne();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            socketSemaphore.Release();
            socketSemaphore.Release();
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