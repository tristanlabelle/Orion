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
        #region Private
        #region Constants
        private static readonly TimeSpan PacketSessionTimeout = new TimeSpan(0, 0, 30);
        private static readonly TimeSpan DefaultPacketResendDelay = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan MinimumPacketResendDelay = TimeSpan.FromMilliseconds(20);

        /// <summary>
        /// Winsock error raised if the socket didn't receive anything before it timed out.
        /// </summary>
        private const int WSAETIMEDOUT = 10060;
        #endregion

        private readonly List<PeerLink> peers = new List<PeerLink>();

        private readonly Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Semaphore socketSemaphore = new Semaphore(2, 2);

        private readonly Thread senderThread;
        private readonly Thread receiverThread;


        private volatile bool isDisposed;
        #endregion

        #region Public
        /// <summary>
        /// The port on which the UDP socket is bound.
        /// </summary>
        public readonly int Port;
        #endregion
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
            Port = port;
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);

            senderThread = new Thread(SenderThreadEntryPoint);
            senderThread.Name = "Sender Thread for {0}".FormatInvariant(this);
            senderThread.IsBackground = true;
            receiverThread = new Thread(ReceiverThreadEntryPoint);
            receiverThread.Name = "Receiver Thread for {0}".FormatInvariant(this);
            receiverThread.IsBackground = true;

            receiverThread.Start();
            senderThread.Start();
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

        private void OnReceived(NetworkEventArgs eventArgs)
        {
            if (Received != null) Received(this, eventArgs);
        }

        private void OnTimedOut(IPv4EndPoint endPoint)
        {
            if (TimedOut != null) TimedOut(this, endPoint);
        }
        #endregion

        #region Methods
        private PeerLink GetPeerLink(IPv4EndPoint ipEndPoint)
        {
            lock (peers)
            {
                PeerLink peer = peers.FirstOrDefault(p => p.EndPoint == ipEndPoint);
                if (peer == null)
                {
                    peer = new PeerLink(ipEndPoint);
                    peers.Add(peer);
                }
                return peer;
            }
        }

        #region Receiver Thread
        private void ReceiverThreadEntryPoint()
        {
            byte[] packet = new byte[1024];

            while (true)
            {
                try
                {
                    IPv4EndPoint? hostEndPoint;
                    int packetSizeInBytes = WaitForPacket(packet, out hostEndPoint);
                    if (!hostEndPoint.HasValue) break;

                    PeerLink peer = GetPeerLink(hostEndPoint.Value);
                    if (peer.HasTimedOut)
                    {
                        Debug.Fail(
                            "Timed out peer {0} has revived."
                            .FormatInvariant(hostEndPoint.Value));
                        continue;
                    }

                    peer.HandlePacket(packet);
                    if (packet[0] == (byte)PacketType.Data)
                    {
                        // it is always necessary to send an answer to data packets
                        #region Send The Ack back
                        byte[] answer = new byte[5];
                        answer[0] = (byte)PacketType.Acknowledgement;
                        for (int i = 1; i < 5; i++)
                            answer[i] = packet[i];
                        socketSemaphore.WaitOne();
                        try
                        {
                            if (isDisposed) break;
                            udpSocket.SendTo(answer, hostEndPoint);
                        }
                        finally
                        {
                            socketSemaphore.Release();
                        }
                        #endregion
                    }
                    else
                    {
                        // we don't want the game to crash on us if we receive something malformed from a potentially unknown host
                        Console.WriteLine("*** Safe Transporter received an unknown packet of type {0}", packet[0]);
                    }
                }
                catch (SocketException exception)
                {
                    Debug.Fail("Unexpected socket exception {0}: {1}".FormatInvariant(exception.ErrorCode, exception));
                    break;
                }

                Array.Clear(packet, 0, packet.Length);
            }
        }

        private int WaitForPacket(byte[] packet, out IPv4EndPoint? hostEndPoint)
        {
            EndPoint endPoint = new IPEndPoint(0, 0);
            hostEndPoint = null;

            while (true)
            {
                try
                {
                    if (isDisposed) return -1;

                    socketSemaphore.WaitOne();
                    int packetSizeInBytes = udpSocket.ReceiveFrom(packet, ref endPoint);
                    hostEndPoint = (IPv4EndPoint)(IPEndPoint)endPoint;
                    return packetSizeInBytes;
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
        }
        #endregion

        #region Sender Thread
        private void SenderThreadEntryPoint()
        {
            while (!isDisposed)
            {
                socketSemaphore.WaitOne();
                try
                {
                    lock (peers)
                    {
                        foreach (PeerLink peer in peers)
                        {
                            if (peer.HasTimedOut) continue;
                            foreach (SafePacketSession session in peer.PacketsToSend)
                            {
                                if (session.TimeElapsedSinceCreation > PacketSessionTimeout)
                                {
                                    peer.MarkAsTimedOut();
                                    break;
                                }

                                TimeSpan resendDelay = GetResendDelay(session.ID.HostEndPoint);
                                if (!session.WasSent || session.TimeElapsedSinceLastSend >= resendDelay)
                                    session.Send(udpSocket);
                            }
                        }



                    }

                }
                catch (SocketException exception)
                {
                    Debug.Fail("Unexpected socket exception {0}: {1}".FormatInvariant(exception.ErrorCode, exception));
                    break;
                }
                finally
                {
                    socketSemaphore.Release();
                }

                Thread.Sleep(0);
            }
        }

        private TimeSpan GetResendDelay(IPv4EndPoint hostEndPoint)
        {
            PeerLink peer = GetPeerLink(hostEndPoint);
            if (!peer.HasPingData) return DefaultPacketResendDelay;
            TimeSpan resendDelay = peer.AveragePing + peer.AveragePingDeviation;
            return resendDelay < MinimumPacketResendDelay ? MinimumPacketResendDelay : resendDelay;
        }
        #endregion

        #region Sending
        /// <summary>
        /// Sends data to a specified host.
        /// </summary>
        /// <param name="data">
        /// The data to send.
        /// </param>
        /// <param name="remoteHost">
        /// The host to which the data is addressed.
        /// </param>
        public void SendTo(byte[] data, IPv4EndPoint hostEndPoint)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(hostEndPoint, "hostEndPoint");
            PeerLink peer = GetPeerLink(hostEndPoint);

        }

        public void SendTo(byte[] data, IEnumerable<IPv4EndPoint> hostEndPoints)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(hostEndPoints, "hostEndPoints");

            foreach (IPv4EndPoint endPoint in hostEndPoints)
                SendTo(data, endPoint);
        }

        /// <summary>
        /// Broadcasts data using the standard packet format to all listening safe transporters.
        /// </summary>
        /// <remarks>
        /// Broadcasted packets inherently cannot be tracked. Consequently, they must be sent only once,
        /// and do not imply an acknowledge from any host. Hosts willing to answer must initiate a new
        /// controlled session through the standard SendTo method with this host's address.
        /// </remarks>
        /// <param name="data">The data to broadcast</param>
        /// <param name="port">The port on which to broadcast</param>
        public void Broadcast(byte[] data, int port)
        {
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureWithin(port, 1, ushort.MaxValue, "port");

            // This was the implementation before Mathieu trashed away SendTo (and thus invalidated this code).
            // Please fix me!
            
            /*
            IPv4EndPoint broadcastAddress = new IPv4EndPoint((IPv4Address)IPAddress.Broadcast, port);
            SafePacketID id = new SafePacketID(broadcastAddress, 0);
            SafePacketSession broadcastSession = new SafePacketSession(id, PacketType.Broadcast, data);

            broadcastSession.Send(udpSocket);
            */
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

        private void RaiseReceivedEvents()
        {
            lock (peers)
            {
                foreach (PeerLink peer in peers)
                {
                    while (!peer.HasReadyData) continue;
                    {
                        OnReceived(new NetworkEventArgs(peer.EndPoint, peer.getNextReadyData()));
                    }
                }
            }
        }

        private void RaiseTimedOutEvents()
        {
            lock (peers)
            {
                foreach (PeerLink peer in peers.Where(p => p.HasTimedOut))
                    OnTimedOut(peer.EndPoint);
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

            udpSocket.Shutdown(SocketShutdown.Both);
            udpSocket.Close();

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