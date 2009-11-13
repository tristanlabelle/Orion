using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Orion.Networking
{
    /// <summary>
    /// Represent a connection to a distant computer (peer)
    /// </summary>
    internal sealed class PeerLink
    {
        #region Fields
        private readonly IPv4EndPoint endPoint;
        private readonly Queue<TimeSpan> pings = new  Queue<TimeSpan>();
        private readonly SortedList<uint, byte[]> readyData = new SortedList<uint, byte[]>();
        private readonly SortedList<uint, NetworkTimeoutEventArgs> timedOut = new SortedList<uint, NetworkTimeoutEventArgs>();
        private readonly List<uint> acknowledgedPackets = new List<uint>();
        private volatile uint expectedPacketNumber;
        private volatile uint nextPacketNumber;
        private volatile bool hasTimedOut;

        private readonly Dictionary<uint,SafePacketSession> packetsToSend
           = new Dictionary<uint, SafePacketSession>();



        #endregion

        #region Constructor
        public PeerLink(IPv4EndPoint endPoint)
        {
            this.endPoint = endPoint;
        }
        #endregion

        #region Proprieties

        public bool HasTimedOut
        {
            get { return hasTimedOut; }
        }
        public bool HasReadyData
        {
            get { return readyData.Count>0 && readyData.Keys.First() == expectedPacketNumber; }
        }

        public IEnumerable<SafePacketSession> PacketsToSend
        {
            get { return packetsToSend.Values; }
        } 


        public uint NextPacketNumber
        {
            get { return nextPacketNumber; }
        }

        public uint ExpectedPacketNumber
        {
            get { return expectedPacketNumber; }
        }

        public IPv4EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public bool HasPingData
        {
            get { return pings.Count > 0; }
        }

        public TimeSpan AveragePing
        {
            get
            {
                lock (pings)
                {
                    if (pings.Count == 0) return TimeSpan.Zero;
                    return LocklessAveragePing;
                }
            }
        }

        private TimeSpan LocklessAveragePing
        {
            get
            {
                long averageTicks = (long)pings.Average(timeSpan => timeSpan.Ticks);
                return TimeSpan.FromTicks(averageTicks);
            }
        }

        public TimeSpan AveragePingDeviation
        {
            get
            {
                lock (pings)
                {
                    if (pings.Count == 0) return TimeSpan.Zero;

                    TimeSpan averagePing = LocklessAveragePing;

                    long deviationSumInTicks = pings.Sum(ping => Math.Abs(averagePing.Ticks - ping.Ticks));
                    long averageDeviationInTicks = deviationSumInTicks / pings.Count;
                    return TimeSpan.FromTicks(averageDeviationInTicks);
                }
            }
        }
        #endregion

        #region Methods
        #region Ping Functions
        public void AddPing(TimeSpan timeSpan)
        {
            lock (pings)
            {
                pings.Enqueue(timeSpan);
                if (pings.Count > 50) pings.Dequeue();
            }
        }
        #endregion

        #region Recieve Functions
        public void HandlePacket(byte[] packet)
        {
           

            uint sessionID = BitConverter.ToUInt32(packet, 1);

            if (packet[0] == (byte)PacketType.Data)
            {

                lock (acknowledgedPackets)
                {
                    if (!acknowledgedPackets.Contains(sessionID))
                        acknowledgedPackets.Add(sessionID);
                }

                lock (readyData)
                {
                    ushort packetLength = BitConverter.ToUInt16(packet, 1 + sizeof(uint));

                    byte[] packetData = new byte[packetLength];
                    Array.Copy(packet, 1 + sizeof(uint) + sizeof(ushort), packetData, 0, packetLength);

                    readyData.Add(sessionID, packetData);
                }
            }
            else if (packet[0] == (byte)PacketType.Acknowledgement)
            {
                lock (packetsToSend)
                {
                    if (packetsToSend.ContainsKey(sessionID))
                    {
                        AddPing(packetsToSend[sessionID].TimeElapsedSinceCreation);
                        packetsToSend.Remove(sessionID);
                    }
                }
            }
        }
        #endregion

        public SafePacketSession CreatePacketSession(byte[] data)
        {
            uint packetNumber = nextPacketNumber;
            ++nextPacketNumber;

            lock (packetsToSend)
            {
                SafePacketID id = new SafePacketID(endPoint, packetNumber);
                SafePacketSession session = new SafePacketSession(id, data);
                packetsToSend.Add(packetNumber,session);
                return session;
            }
        }

        public void MarkAsTimedOut()
        {
            hasTimedOut = true;
            packetsToSend.Clear();
            readyData.Clear();
            pings.Clear();
            acknowledgedPackets.Clear();
        }

        public byte[] getNextReadyData()
        {
            byte[] nextReadyData = readyData.First().Value;
            ++expectedPacketNumber;
            return nextReadyData;
        }
        #endregion
    }
}
