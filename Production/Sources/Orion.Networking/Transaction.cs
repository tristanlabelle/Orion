﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    internal abstract class Transaction
    {
        #region Fields

        protected static DateTime BigBang = new DateTime(0);

        protected DateTime ResendTimeout;
        protected DateTime TransactionTimeout;
        protected byte packetId;
        protected Transporter Transporter;
        protected Dictionary<byte, DateTime> packetSendTimes;

        public readonly IPEndPoint RemoteHost;

        #endregion

        #region Constructors

        public Transaction(Transporter transporter, IPEndPoint host)
        {
            Transporter = transporter;
            RemoteHost = host;
            packetSendTimes = new List<DateTime>();
            TransactionTimeout = DateTime.MaxValue;
        }

        #endregion

        #region Properties

        public bool IsReady 
        {
            get { return DateTime.UtcNow > ResendTimeout && !IsCompleted; }
        }

        public bool HasTimedOut
        {
            get { return DateTime.UtcNow > TransactionTimeout; }
        }

        public abstract bool IsCompleted { get; }

        #endregion

        #region Methods

        protected void ResetTransactionTimeout()
        {
            TransactionTimeout = DateTime.UtcNow + new TimeSpan(0, 0, 5);
        }

        protected void ResetSendingTimeout()
        {
            ResendTimeout = DateTime.UtcNow + Transporter.AverageAnswerTimeForHost(RemoteHost);
        }

        public abstract void Receive(byte[] data);
        public abstract byte[] Send();

        #endregion
    }
}