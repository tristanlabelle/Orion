using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        private readonly Queue<byte[]> receivedBroadcastMessages = new Queue<byte[]>();
        private readonly SortedList<uint, byte[]> receivedSequencialMessages = new SortedList<uint, byte[]>();
        private readonly List<uint> acknowledgedPackets = new List<uint>();
        private volatile uint expectedPacketNumber;
        private volatile uint nextPacketNumber;
        private volatile bool hasTimedOut;

        private readonly Dictionary<uint,SafePacket> packetsToSend
           = new Dictionary<uint, SafePacket>();

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
            get
            {
                return receivedBroadcastMessages.Count > 0
                    || (receivedSequencialMessages.Count > 0
                    && receivedSequencialMessages.Keys.First() == expectedPacketNumber);
            }
        }

        public IEnumerable<SafePacket> PacketsToSend
        {
            get { return packetsToSend.Values; }
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
        public void HandlePacket(byte[] data, int dataLength)
        {
            PacketType type = Protocol.GetPacketType(data);
            if (type == PacketType.Data)
            {
                uint packetNumber = Protocol.GetDataPacketNumber(data);
                if (packetNumber < expectedPacketNumber || receivedSequencialMessages.ContainsKey(packetNumber))
                {
                    Debug.Fail("Received the same packet twice.");
                    return;
                }

                lock (acknowledgedPackets)
                {
                    if (!acknowledgedPackets.Contains(packetNumber))
                        acknowledgedPackets.Add(packetNumber);
                }

                byte[] message = Protocol.GetDataPacketMessage(data, dataLength);

                lock (receivedSequencialMessages)
                {
                    receivedSequencialMessages.Add(packetNumber, message);
                }
            }
            else if (type == PacketType.Acknowledgement)
            {
                uint packetNumber = Protocol.GetAcknowledgementPacketNumber(data);
                AcknowledgePacket(packetNumber);
            }
            else if (type == PacketType.Broadcast)
            {
                byte[] message = Protocol.GetBroadcastPacketMessage(data, dataLength);
                receivedBroadcastMessages.Enqueue(message);
            }
        }
        #endregion

        public SafePacket CreatePacket(byte[] data)
        {
            uint packetNumber = nextPacketNumber;
            ++nextPacketNumber;

            lock (packetsToSend)
            {
                SafePacket packet = new SafePacket(packetNumber, data);
                packetsToSend.Add(packetNumber, packet);
                return packet;
            }
        }

        public void AcknowledgePacket(uint number)
        {
            if (packetsToSend.ContainsKey(number))
            {
                AddPing(packetsToSend[number].TimeElapsedSinceCreation);
                packetsToSend.Remove(number);
            }
        }

        public void MarkAsTimedOut()
        {
            hasTimedOut = true;
            packetsToSend.Clear();
            receivedSequencialMessages.Clear();
            pings.Clear();
            acknowledgedPackets.Clear();
        }

        public byte[] PopNextReadyMessage()
        {
            if (receivedBroadcastMessages.Count > 0)
                return receivedBroadcastMessages.Dequeue();

            var pair = receivedSequencialMessages.First();
            receivedSequencialMessages.Remove(pair.Key);
            ++expectedPacketNumber;
            return pair.Value;
        }
        #endregion
    }
}
