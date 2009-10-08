using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Orion.Networking
{
    public sealed class Transporter : IDisposable
    {
        #region Fields
        #region Private

        private object sessionIdLocker = new object();
        private static uint NextSessionId;

        private readonly Socket udpSocket;
        private readonly Dictionary<IPEndPoint, Queue<TimeSpan>> answerTimes;
        private readonly Dictionary<IPEndPoint, Dictionary<uint, Transaction>> transactions;
        private readonly Thread senderThread;
        private readonly Thread receiverThread;
        private bool isDisposed;

        #endregion
        
        #region Public

        public readonly int Port;

        #endregion
        #endregion

        #region Events

        public event GenericEventHandler<Transporter, NetworkEventArgs> Received;

        #endregion

        #region Constructor

        public Transporter(int port)
        {
            Port = port;
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            transactions = new Dictionary<IPEndPoint, Dictionary<uint, Transaction>>();
            answerTimes = new Dictionary<IPEndPoint, Queue<TimeSpan>>();
            senderThread = new Thread(SenderThread);
            receiverThread = new Thread(ReceiverThread);
        }

        #endregion

        #region Methods

        #region Private

        private void CheckIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private void SenderThread()
        {
            while (true)
            {
                if (isDisposed) break;

                lock (transactions)
                {
                    foreach(KeyValuePair<IPEndPoint, Dictionary<uint, Transaction>> hostTransactions in transactions)
                    {
                        IPEndPoint remoteHost = hostTransactions.Key;
                        foreach (KeyValuePair<uint, Transaction> pair in hostTransactions.Value)
                        {
                            Transaction transaction = pair.Value;
                            if (transaction.IsReady)
                            {
                                uint sessionId = pair.Key;
                                if (transaction is SendingTransaction)
                                {
                                    sessionId |= 0x80000000;
                                }

                                byte[] dataToSend = transaction.Send();
                                byte[] packetData = new byte[sizeof(int) + dataToSend.Length];

                                BitConverter.GetBytes(sessionId).CopyTo(packetData, 0);
                                dataToSend.CopyTo(packetData, sizeof(int));
                                udpSocket.SendTo(packetData, remoteHost);
                            }
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void ReceiverThread()
        {
            byte[] buffer = new byte[512];
            EndPoint endpointFrom = null;
            while (true)
            {
                if (isDisposed) break;

                int size = udpSocket.ReceiveFrom(buffer, ref endpointFrom);
                IPEndPoint from = endpointFrom as IPEndPoint;
            }
        }

        #endregion

        #region Internal

        internal TimeSpan AverageAnswerTimeForHost(IPEndPoint host)
        {
            double result = 0;
            foreach (TimeSpan answerTime in answerTimes[host])
            {
                result += answerTime.TotalMilliseconds;
            }

            return new TimeSpan(0, 0, 0, 0, (int)result);
        }

        #endregion

        #region Public

        public void SendTo(byte[] data, IPEndPoint to)
        {
            CheckIfDisposed();
            SendingTransaction transaction = new SendingTransaction(this, to, data);
            uint sessionId;
            lock (sessionIdLocker)
            {
                sessionId = NextSessionId;
                NextSessionId++;
                NextSessionId &= 0x7FFFFFFF;
            }

            lock (transactions)
            {
                if (!transactions.ContainsKey(to))
                {
                    transactions[to] = new Dictionary<uint, Transaction>();
                }

                transactions[to][sessionId] = transaction;
            }
        }

        public void Dispose()
        {
            CheckIfDisposed();
            isDisposed = true;
        }

        #endregion

        #endregion
    }

}
