using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;

namespace Orion.Engine.Networking
{
    /// <summary>
    /// Holds all data needed to garantee the delivery of a packet of data.
    /// </summary>
    internal sealed class SafePacket
    {
        #region Fields
        private readonly DateTime creationTime = DateTime.UtcNow;
        private readonly byte[] data;
        private DateTime? lastSendTime;
        #endregion

        #region Constructors
        public SafePacket(uint number, byte[] message)
        {
            Argument.EnsureNotNull(message, "message");

            this.data = Protocol.CreateDataPacket(message, number);
        }
        #endregion

        #region Properties
        public uint Number
        {
            get { return Protocol.GetDataPacketNumber(data); }
        }

        public byte[] Data
        {
            get { return data; }
        }

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
                return DateTime.UtcNow - lastSendTime.Value;
            }
        }

        public bool WasSent
        {
            get { return lastSendTime.HasValue; }
        }
        #endregion

        #region Methods
        public void UpdateSendTime()
        {
            lastSendTime = DateTime.UtcNow;
        }
        #endregion
    }
}
