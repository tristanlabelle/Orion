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

        private object locker = new object();
        private static int NextSessionId;

        private readonly Socket udpSocket;
        private readonly Dictionary<IPEndPoint, Queue<TimeSpan>> answerTimes;
        private readonly Dictionary<IPEndPoint, Dictionary<int, Transaction>> transactions;
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
            transactions = new Dictionary<IPEndPoint, Dictionary<int, Transaction>>();
            answerTimes = new Dictionary<IPEndPoint, Queue<TimeSpan>>();
            senderThread = new Thread(SenderThread);
            receiverThread = new Thread(ReceiverThread);
        }

        #endregion

        #region Methods

        #region Private
        private void SenderThread()
        {
            while (true)
            {
                if (isDisposed)
                    break;
            }
        }

        private void ReceiverThread()
        {
            while (true)
            {
                if (isDisposed)
                    break;
            }
        }

        private void CheckIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(null);
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
            lock (locker)
            {
                int sessionId = NextSessionId;
                NextSessionId++;

                if (!transactions.ContainsKey(to))
                {
                    transactions[to] = new Dictionary<int, Transaction>();
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
