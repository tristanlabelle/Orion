using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal abstract class Transaction
    {
        #region Nested Types

        protected sealed class DataHolder
        {
            #region Fields
            public byte[] Bytes;
            #endregion

            #region Constructeurs
            public DataHolder()
            {
                Bytes = new byte[2];
            }

            public DataHolder(byte[] data)
            {
                Bytes = data;
            }

            public DataHolder(byte remotePacketId, byte[] data)
            {
                Bytes = new byte[data.Length + 2];
                RemotePacketId = remotePacketId;
                Data = data;
            }
            #endregion

            #region Properties
            public byte PacketId
            {
                get { return Bytes[0]; }
                set { Bytes[0] = value; }
            }

            public byte RemotePacketId
            {
                get { return Bytes[1]; }
                set { Bytes[1] = value; }
            }

            public int TypeSignature
            {
                get { return BitConverter.ToInt32(Bytes, 2); }
                set { BitConverter.GetBytes(value).CopyTo(Bytes, 2); }
            }

            public byte[] Data
            {
                get { return Bytes.Skip(2).ToArray(); }
                set
                {
                    if (value.Length > Bytes.Length + 2)
                    {
                        byte[] newStore = new byte[value.Length + 2];
                        newStore[0] = PacketId;
                        newStore[1] = RemotePacketId;
                        Bytes = newStore;
                    }
                    value.CopyTo(Bytes, 2);
                }
            }
            #endregion

            #region Methods
            public override string ToString()
            {
                return string.Format("Packet of type {0} (id {1}, in response to {2}) with data {3}", TypeSignature, PacketId, RemotePacketId, Data);
            }
            #endregion
        }

        #endregion

        #region Fields

        private byte nextPacketId;
        protected DateTime resendTimeout;
        protected DateTime transactionTimeout;
        protected Transporter transporter;
        protected Dictionary<byte, DateTime> packetSendTimes;

        public readonly IPEndPoint RemoteHost;

        #endregion

        #region Constructors

        public Transaction(Transporter transporter, IPEndPoint host)
        {
            this.transporter = transporter;
            RemoteHost = host;
            packetSendTimes = new Dictionary<byte, DateTime>();
            transactionTimeout = DateTime.MaxValue;
        }

        #endregion

        #region Properties

        public bool IsReady 
        {
            get { return DateTime.UtcNow > resendTimeout && !IsCompleted; }
        }

        public bool HasTimedOut
        {
            get { return DateTime.UtcNow > transactionTimeout; }
        }

        public abstract bool IsCompleted { get; }
        public abstract byte[] Data { get; }

        #endregion

        #region Methods

        protected void ResetTransactionTimeout()
        {
            transactionTimeout = DateTime.UtcNow + new TimeSpan(0, 0, 5);
        }

        protected void ResetSendingTimeout()
        {
            resendTimeout = DateTime.UtcNow + transporter.AverageAnswerTimeForHost(RemoteHost);
        }

        protected void PrepareSend(DataHolder holder)
        {
            ResetSendingTimeout();
            holder.PacketId = nextPacketId;
            packetSendTimes[holder.PacketId] = DateTime.UtcNow;
            nextPacketId++;
        }

        protected void ReceivedAnswerForPacket(byte packetId)
        {
            transporter.PushAnswerTime(RemoteHost, DateTime.UtcNow - packetSendTimes[packetId]);
            packetSendTimes.Remove(packetId);
        }

        public abstract void Receive(byte[] data);
        public abstract byte[] Send();

        #endregion
    }
}
