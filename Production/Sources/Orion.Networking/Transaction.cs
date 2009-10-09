using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal abstract class Transaction
    {
        #region Fields

        protected static DateTime bigBang = new DateTime(0);

        protected DateTime resendTimeout;
        protected DateTime transactionTimeout = DateTime.MaxValue;
        protected byte packetId;
        protected Transporter transporter;
        protected Dictionary<byte, DateTime> packetSendTimes = new Dictionary<byte, DateTime>();

        public readonly IPEndPoint RemoteHost;

        #endregion

        #region Constructors

        public Transaction(Transporter transporter, IPEndPoint host)
        {
            this.transporter = transporter;
            this.RemoteHost = host;
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

        public abstract void Receive(byte[] data);
        public abstract byte[] Send();

        #endregion
    }
}
