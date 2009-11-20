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
        private readonly SortedList<uint, byte[]> receivedSequencialMessages
            = new SortedList<uint, byte[]>();
        private uint expectedPacketNumber;
        private uint nextPacketNumber;
        private bool hasTimedOut;

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

        public bool HasAvailableMessage
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

                    TimeSpan averagePing = AveragePing;

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

        public bool HasReceivedMessage(uint number)
        {
            return number < expectedPacketNumber
                || receivedSequencialMessages.ContainsKey(number);
        }

        public void AddReceivedMessage(uint number, byte[] message)
        {
            if (HasReceivedMessage(number))
            {
                throw new InvalidOperationException(
                    "Message #{0} received twice.".FormatInvariant(number));
            }

            receivedSequencialMessages.Add(number, message);
        }

        public void AddReceivedBroadcastMessage(byte[] message)
        {
            receivedBroadcastMessages.Enqueue(message);
        }

        public void MarkMessageAsAcknowledged(uint number)
        {
            if (packetsToSend.ContainsKey(number))
            {
                AddPing(packetsToSend[number].TimeElapsedSinceCreation);
                packetsToSend.Remove(number);
            }
            else
            {
                Debug.Fail("Received an acknowledgement for an unsent message.");
            }
        }

        public void MarkAsTimedOut()
        {
            hasTimedOut = true;
            packetsToSend.Clear();
            receivedSequencialMessages.Clear();
            pings.Clear();
        }

        public byte[] PopNextAvailableMessage()
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
