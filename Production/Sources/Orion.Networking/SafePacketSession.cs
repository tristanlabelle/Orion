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
    internal sealed class SafePacketSession
    {
        #region Fields
        private readonly DateTime creationTime = DateTime.UtcNow;
        private DateTime? lastSendTime;
        private readonly byte[] fullPacket;

        public readonly SafePacketID ID;
        public readonly byte[] Data;
        #endregion

        #region Constructors
        public SafePacketSession(SafePacketID id, byte[] data)
        {
            Argument.EnsureNotNull(data, "data");

            this.ID = id;
            this.Data = data;

            fullPacket = new byte[data.Length + 7];
            fullPacket[0] = (byte)PacketType.Data;
            BitConverter.GetBytes(id.SessionID).CopyTo(fullPacket, 1);
            BitConverter.GetBytes((ushort)Data.Length).CopyTo(fullPacket, 1 + sizeof(uint));
            Data.CopyTo(fullPacket, 1 + sizeof(uint) + sizeof(ushort));
        }
        #endregion

        #region Properties
        public DateTime CreationTime
        {
            get { return creationTime; }
        }

        public TimeSpan TimeElapsedSinceCreation
        {
            get { return DateTime.UtcNow - creationTime; }
        }

        public DateTime? LastSendTime
        {
            get { return lastSendTime; }
        }

        public TimeSpan? TimeElapsedSinceLastSend
        {
            get
            {
                if (!lastSendTime.HasValue) return null;
                return DateTime.UtcNow - lastSendTime;
            }
        }

        public bool WasSent
        {
            get { return lastSendTime.HasValue; }
        }
        #endregion

        #region Methods
        public void Send(Socket udpSocket)
        {
            Argument.EnsureNotNull(udpSocket, "udpSocket");

            udpSocket.SendTo(fullPacket, ID.RemoteHost);
            lastSendTime = DateTime.UtcNow;
            Console.WriteLine("Packet #{0} sent at {1}", ID, lastSendTime);
        }
        #endregion
    }
}
