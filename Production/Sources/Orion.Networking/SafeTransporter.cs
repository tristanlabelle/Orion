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
    /// It creates a single UDP socket for communication to various hosts. The remote host must use a Transporter as well for reception.
    /// </summary>
    public sealed class SafeTransporter : IDisposable
    {
        #region Fields
        #region Private
        #region Constants
        private static readonly TimeSpan PacketSessionTimeout = new TimeSpan(0, 0, 30);
        private static readonly TimeSpan DefaultPacketResendDelay = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan MinimumPacketResendDelay = TimeSpan.FromMilliseconds(20);
        private static readonly TimeSpan DefaultPing = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Winsock error raised if the socket didn't receive anything before it timed out.
        /// </summary>
        private const int WSAETIMEDOUT = 10060;
        #endregion

        private uint nextSessionID;

        private readonly Queue<NetworkEventArgs> readyData = new Queue<NetworkEventArgs>();
        private readonly Queue<NetworkTimeoutEventArgs> timedOut = new Queue<NetworkTimeoutEventArgs>();

        private readonly Dictionary<Ipv4EndPoint, Queue<TimeSpan>> pings
            = new Dictionary<Ipv4EndPoint, Queue<TimeSpan>>();
        private readonly List<SafePacketID> acknowledgedPackets = new List<SafePacketID>();

        private readonly Dictionary<SafePacketID, SafePacketSession> packetsToSend
            = new Dictionary<SafePacketID, SafePacketSession>();

        private readonly Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Semaphore socketSemaphore = new Semaphore(2, 2);

        private readonly Thread senderThread;
        private readonly Thread receiverThread;

        private bool isDisposed;
        #endregion

        #region Public
        /// <summary>
        /// The port on which the UDP socket is bound.
        /// </summary>
        public readonly int Port;
        #endregion
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
        public event GenericEventHandler<SafeTransporter, NetworkTimeoutEventArgs> TimedOut;

        private void OnReceived(NetworkEventArgs eventArgs)
        {
            if (Received != null) Received(this, eventArgs);
        }

        private void OnTimedOut(NetworkTimeoutEventArgs eventArgs)
        {
            if (TimedOut != null) TimedOut(this, eventArgs);
        }
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
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);
            senderThread = new Thread(SenderThread);
            receiverThread = new Thread(ReceiverThread);

            senderThread.Name = string.Format("Sender Thread for {0}", this);
            receiverThread.Name = string.Format("Receiver Thread for {0}", this);

            receiverThread.Start();
            senderThread.Start();
        }
        #endregion

        #region Methods
        #region Ping calculation methods
        private void AddPing(Ipv4EndPoint hostEndPoint, TimeSpan timeSpan)
        {
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                if (!pings.ContainsKey(hostEndPoint))
                {
                    pings[hostEndPoint] = new Queue<TimeSpan>();
                }
                hostPings = pings[hostEndPoint];
            }

            lock (hostPings)
            {
                hostPings.Enqueue(timeSpan);
                if (hostPings.Count > 50)
                {
                    hostPings.Dequeue();
                }
            }
        }

        private TimeSpan AveragePing(Ipv4EndPoint hostEndPoint)
        {
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                if (!pings.ContainsKey(hostEndPoint))
                {
                    pings[hostEndPoint] = new Queue<TimeSpan>();
                }
                hostPings = pings[hostEndPoint];
            }

            lock (hostPings)
            {
                if (hostPings.Count == 0) return DefaultPing;

                long averageTicks = (long)hostPings.Average(timeSpan => timeSpan.Ticks);
                return TimeSpan.FromTicks(averageTicks);
            }
        }

        private TimeSpan StandardDeviationForPings(Ipv4EndPoint hostEndPoint)
        {
            long deviationInTicks = 0;
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                hostPings = pings[hostEndPoint];
            }

            if (hostPings.Count == 0) return TimeSpan.FromMilliseconds(50);

            TimeSpan average = AveragePing(hostEndPoint);
            lock (hostPings)
            {
                foreach (TimeSpan ping in hostPings)
                    deviationInTicks += Math.Abs(average.Ticks - ping.Ticks);
                
                deviationInTicks /= hostPings.Count;
            }

            return TimeSpan.FromTicks(deviationInTicks);
        }

        #endregion

        #region Receiver Thread
        private void ReceiverThread()
        {
            byte[] answer = new byte[5];
            answer[0] = (byte)PacketType.Acknowledgement;

            byte[] packet = new byte[1024];

            while (true)
            {
                try
                {
                    Ipv4EndPoint? hostEndPoint;
                    int packetSizeInBytes = WaitForPacket(packet, out hostEndPoint);
                    if (!hostEndPoint.HasValue) break;

                    uint sessionID = BitConverter.ToUInt32(packet, 1);
                    SafePacketID id = new SafePacketID(hostEndPoint.Value, sessionID);

                    if (packet[0] == (byte)PacketType.Data)
                    {
                        // copy the session id
                        for (int i = 1; i < 5; i++)
                            answer[i] = packet[i];

                        // it is always necessary to send an answer to data packets
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

                        lock (acknowledgedPackets)
                        {
                            if (acknowledgedPackets.Contains(id)) continue;
                            acknowledgedPackets.Add(id);
                        }

                        lock (readyData)
                        {
                            ushort packetLength = BitConverter.ToUInt16(packet, 1 + sizeof(uint));
                            byte[] packetData = new byte[packetLength];
                            Array.Copy(packet, 1 + sizeof(uint) + sizeof(ushort), packetData, 0, packetLength);
                            readyData.Enqueue(new NetworkEventArgs(id.HostEndPoint, packetData));
                        }
                    }
                    else if (packet[0] == (byte)PacketType.Acknowledgement)
                    {
                        lock (packetsToSend)
                        {
                            if (packetsToSend.ContainsKey(id))
                            {
                                AddPing(id.HostEndPoint, packetsToSend[id].TimeElapsedSinceCreation);
                                packetsToSend.Remove(id);
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Unexpected packet type received.");
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Broke from socket exception {0}: {1}", e.ErrorCode, e);
                    break;
                }

                Array.Clear(packet, 0, packet.Length);
            }
        }

        private int WaitForPacket(byte[] packet, out Ipv4EndPoint? hostEndPoint)
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
                    hostEndPoint = (Ipv4EndPoint)(IPEndPoint)endPoint;
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
        private void SenderThread()
        {
            List<SafePacketSession> trash = new List<SafePacketSession>();
            List<SafePacketSession> sessions = new List<SafePacketSession>();
            while (true)
            {
                if (isDisposed) break;
                socketSemaphore.WaitOne();

                try
                {
                    sessions.Clear();
                    lock (packetsToSend)
                    {
                        sessions.AddRange(packetsToSend.Values);
                    }

                    foreach (SafePacketSession session in sessions)
                    {
                        if (session.TimeElapsedSinceCreation > PacketSessionTimeout)
                        {
                            trash.Add(session);
                            lock (timedOut)
                            {
                                timedOut.Enqueue(new NetworkTimeoutEventArgs(session.ID.HostEndPoint, session.Data));
                            }
                        }

                        TimeSpan resendDelay = GetResendDelay(session.ID.HostEndPoint);
                        if (!session.WasSent || session.TimeElapsedSinceLastSend >= resendDelay)
                            session.Send(udpSocket);
                    }

                    lock (packetsToSend)
                    {
                        foreach (SafePacketSession session in trash)
                            packetsToSend.Remove(session.ID);

                        trash.Clear();
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Broke from socket exception {0}: {1}", e.ErrorCode, e);
                    break;
                }
                finally
                {
                    socketSemaphore.Release();
                }

                Thread.Sleep(10);
            }
        }

        private TimeSpan GetResendDelay(Ipv4EndPoint hostEndPoint)
        {
            if (pings.Count == 0) return DefaultPacketResendDelay;
            TimeSpan resendDelay = AveragePing(hostEndPoint) + StandardDeviationForPings(hostEndPoint);
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
        public void SendTo(byte[] data, Ipv4EndPoint hostEndPoint)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(hostEndPoint, "hostEndPoint");

            uint sessionID = nextSessionID;
            ++nextSessionID;

            lock (packetsToSend)
            {
                SafePacketID id = new SafePacketID(hostEndPoint, sessionID);
                packetsToSend[id] = new SafePacketSession(id, data);
            }
        }

        public void SendTo(byte[] data, IEnumerable<Ipv4EndPoint> hostEndPoints)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(hostEndPoints, "hostEndPoints");

            foreach (Ipv4EndPoint endPoint in hostEndPoints)
                SendTo(data, endPoint);
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
            lock (readyData)
            {
                while (readyData.Count > 0)
                {
                    OnReceived(readyData.Dequeue());
                }
            }
        }

        private void RaiseTimedOutEvents()
        {
            lock (timedOut)
            {
                while (timedOut.Count > 0)
                {
                    OnTimedOut(timedOut.Dequeue());
                }
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