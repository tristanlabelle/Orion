using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;

namespace Orion.Networking
{
    /// <summary>
    /// Holds all data needed to garantee the delivery of a packet of data.
    /// </summary>
    internal sealed class PacketSession
    {
        #region Fields
        private static readonly TimeSpan expiration = new TimeSpan(0, 0, 5);

        private readonly DateTime creationTime = DateTime.UtcNow;
        private readonly Stopwatch timeToReceive = Stopwatch.StartNew();
        private DateTime whenToResend;
        private readonly byte[] fullPacket;

        public readonly PacketID ID;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public PacketSession(PacketID id, byte[] data)
        {
            Argument.EnsureNotNull(data, "data");

            this.whenToResend = creationTime;
            this.ID = id;
            this.Data = data;

            fullPacket = new byte[data.Length + 7];
            fullPacket[0] = (byte)PacketType.Data;
            BitConverter.GetBytes(id.SessionID).CopyTo(fullPacket, 1);
            BitConverter.GetBytes((ushort)Data.Length).CopyTo(fullPacket, 1 + sizeof(uint));
            Data.CopyTo(fullPacket, 1 + sizeof(uint) + sizeof(ushort));
        }
        #endregion

        #region Methods
        public void SendThrough(Socket udpSocket)
        {
            Argument.EnsureNotNull(udpSocket, "udpSocket");

            timeToReceive.Reset();
            timeToReceive.Start();
            udpSocket.SendTo(fullPacket, ID.RemoteHost);
        }

        public void ResetSendTime(TimeSpan delay)
        {
            whenToResend = DateTime.UtcNow + delay;
        }

        public bool NeedsResend
        {
            get { return DateTime.UtcNow >= whenToResend; }
        }

        public bool HasTimedOut
        {
            get { return whenToResend - creationTime > expiration; }
        }

        public TimeSpan Acknowledge()
        {
            timeToReceive.Stop();
            return timeToReceive.Elapsed;
        }
        #endregion
    }
}
