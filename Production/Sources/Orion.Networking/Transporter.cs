using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Orion.Networking
{
    /// <summary>
    /// A Transporter is responsible for safely transporting UDP packets over a network.
    /// It guarantees that packets are going to arrive without loss or duplication and as sent.
    /// The order of arrival is not garanteed.
    /// It creates a single UDP socket for communication to various hosts. The remote host must use a Transporter as well for reception.
    /// </summary>
    public sealed class Transporter : IDisposable
    {
        #region Fields
        #region Private
        #region Constants
        private static readonly TimeSpan DefaultPing = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Winsock error raised if the socket didn't receive anything before it timed out.
        /// </summary>
        private const int WSAETIMEDOUT = 10060;
        #endregion

        private uint nextSessionId;

        private readonly Queue<NetworkEventArgs> readyData = new Queue<NetworkEventArgs>();
        private readonly Queue<NetworkTimeoutEventArgs> timedOut = new Queue<NetworkTimeoutEventArgs>();

        private readonly Dictionary<IPEndPoint, Queue<TimeSpan>> pings = new Dictionary<IPEndPoint, Queue<TimeSpan>>();
        private readonly List<PacketID> acknowledgedPackets = new List<PacketID>();

        private readonly Dictionary<PacketID, PacketSession> packetsToSend = new Dictionary<PacketID, PacketSession>();

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
        public event GenericEventHandler<Transporter, NetworkEventArgs> Received;

        /// <summary>
        /// Raised when a packet cannot reach its destination.
        /// </summary>
        /// <remarks>This event is only raised when the method <see cref="M:Poll"/> is called.</remarks>
        public event GenericEventHandler<Transporter, NetworkTimeoutEventArgs> TimedOut;

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
        public Transporter(int port)
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
        private void AddPing(IPEndPoint host, TimeSpan timeSpan)
        {
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                if (!pings.ContainsKey(host))
                {
                    pings[host] = new Queue<TimeSpan>();
                }
                hostPings = pings[host];
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

        private TimeSpan AveragePing(IPEndPoint host)
        {
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                if (!pings.ContainsKey(host))
                {
                    pings[host] = new Queue<TimeSpan>();
                }
                hostPings = pings[host];
            }

            lock (hostPings)
            {
                if (hostPings.Count == 0) return DefaultPing;

                long averageTicks = (long)hostPings.Average(timeSpan => timeSpan.Ticks);
                return TimeSpan.FromTicks(averageTicks);
            }
        }

        private TimeSpan StandardDeviationForPings(IPEndPoint host)
        {
            long deviationInTicks = 0;
            Queue<TimeSpan> hostPings;
            lock (pings)
            {
                hostPings = pings[host];
            }

            if (hostPings.Count == 0) return TimeSpan.FromMilliseconds(50);

            TimeSpan average = AveragePing(host);
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
                    IPEndPoint endPoint = WaitForPacket(packet);
                    if (endPoint == null) break;

                    uint sessionID = BitConverter.ToUInt32(packet, 1);
                    PacketID id = new PacketID(endPoint, sessionID);

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
                            udpSocket.SendTo(answer, endPoint);
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
                            readyData.Enqueue(new NetworkEventArgs(id.RemoteHost, packetData));
                        }
                    }
                    else
                    {
                        lock (packetsToSend)
                        {
                            if (packetsToSend.ContainsKey(id))
                            {
                                AddPing(id.RemoteHost, packetsToSend[id].Acknowledge());
                                packetsToSend.Remove(id);
                            }
                        }
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

        private IPEndPoint WaitForPacket(byte[] packet)
        {
            EndPoint endpoint = new IPEndPoint(0, 0);

            while (true)
            {
                try
                {
                    if (isDisposed) return null;

                    socketSemaphore.WaitOne();
                    udpSocket.ReceiveFrom(packet, ref endpoint);
                    return endpoint as IPEndPoint;
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
            List<PacketSession> trash = new List<PacketSession>();
            List<PacketSession> sessions = new List<PacketSession>();
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

                    foreach (PacketSession session in sessions)
                    {
                        if (session.HasTimedOut)
                        {
                            trash.Add(session);
                            lock (timedOut)
                            {
                                timedOut.Enqueue(new NetworkTimeoutEventArgs(session.ID.RemoteHost, session.Data));
                            }
                        }

                        if (session.NeedsResend)
                        {
                            session.SendThrough(udpSocket);
                            session.ResetSendTime(AveragePing(session.ID.RemoteHost) + StandardDeviationForPings(session.ID.RemoteHost));
                        }
                    }

                    lock (packetsToSend)
                    {
                        foreach (PacketSession session in trash)
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
        public void SendTo(byte[] data, IPEndPoint remoteHost)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(remoteHost, "remoteHost");

            uint sessionID = nextSessionId;
            nextSessionId++;

            lock (packetsToSend)
            {
                PacketID id = new PacketID(remoteHost, sessionID);
                packetsToSend[id] = new PacketSession(id, data);
            }
        }

        public void SendTo(byte[] data, IEnumerable<IPEndPoint> remoteAddresses)
        {
            EnsureNotDisposed();
            Argument.EnsureNotNull(data, "data");
            Argument.EnsureNotNull(remoteAddresses, "remoteAddresses");

            foreach (IPEndPoint endPoint in remoteAddresses)
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