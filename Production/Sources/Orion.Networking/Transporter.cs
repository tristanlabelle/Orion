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
		private const uint localSessionMask = 0x80000000;
        private static uint nextSessionId;

		private readonly Queue<ReceivingTransaction> completedTransactions = new Queue<ReceivingTransaction>();
        private readonly Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Dictionary<IPEndPoint, Queue<TimeSpan>> answerTimes = new Dictionary<IPEndPoint, Queue<TimeSpan>>();
        private readonly Dictionary<IPEndPoint, Dictionary<uint, Transaction>> transactions = new Dictionary<IPEndPoint, Dictionary<uint, Transaction>>();
        private readonly Thread senderThread = new Thread(SenderThread);
        private readonly Thread receiverThread = new Thread(ReceiverThread);
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
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
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
                    foreach(KeyValuePair<IPEndPoint, Dictionary<uint, Transaction>> hostPair in transactions)
                    {
                        IPEndPoint remoteHost = hostPair.Key;
						lock(hostPair.Value)
						{
	                        foreach (KeyValuePair<uint, Transaction> pair in hostPair.Value)
	                        {
	                            Transaction transaction = pair.Value;
								lock(transaction)
								{
		                            if (transaction.IsReady)
		                            {
		                                uint sessionId = pair.Key ^ localSessionMask;
		
		                                byte[] dataToSend = transaction.Send();
		                                byte[] packetData = new byte[sizeof(int) + dataToSend.Length];
		
		                                BitConverter.GetBytes(sessionId).CopyTo(packetData, 0);
		                                dataToSend.CopyTo(packetData, sizeof(int));
		                                udpSocket.SendTo(packetData, remoteHost);
		                            }
								}
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
				
				Dictionary<uint, Transaction> hostTransactions;
				lock(transactions)
				{
					if(!transactions.ContainsKey(from))
					{
						transactions[from] = new Dictionary<uint, Transaction>();
					}
					
					hostTransactions = transactions[from];
				}
				
				uint sessionId = BitConverter.ToUInt32(buffer, 0);
				sessionId ^= localSessionMask;
				byte[] packetData = buffer.Skip(sizeof(uint)).ToArray();
				
				ReceivingTransaction incomingTransaction;
				lock(hostTransactions)
				{
					if(!hostTransactions.ContainsKey(sessionId))
					{
						if((sessionId & localSessionMask) == localSessionMask)
						{
							throw new KeyNotFoundException("Session id {0} should have been local but does not exist", sessionId);
						}
						incomingTransaction = new ReceivingTransaction(this, from);
					}
					else
					{
						incomingTransaction = hostTransactions[sessionId];
					}
				}
				
				lock(incomingTransaction)
				{
					incomingTransaction.Receive(packetData);
					if(incomingTransaction.IsCompleted)
					{
						if(incomingTransaction is ReceivingTransaction)
						{
							lock(completedTransactions)
							{
								completedTransactions.Enqueue(incomingTransaction as ReceivingTransaction);
							}
						}
						
						lock(hostTransactions)
						{
							hostTransactions.Remove(sessionId);
						}
					}
				}
				
				Thread.Sleep(10);
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Returns the average time packets took to reach a host over the last 50 successful tries.
        /// </summary>
        /// <param name="host">The <see cref="IPEndPoint"/> representing the host</param>
        /// <returns>A <see cref="TimeSpan"/> structure indicating the average time it takes to reach the host</returns>
        /// <remarks>If the host was never reached before, this method returns a default value of 100 milliseconds.</remarks>
        internal TimeSpan AverageAnswerTimeForHost(IPEndPoint host)
        {
            if (!answerTimes.ContainsKey(host))
                return new TimeSpan(0, 0, 0, 100);

            double result = 0;
            foreach (TimeSpan answerTime in answerTimes[host])
            {
                result += answerTime.TotalMilliseconds;
            }

            return new TimeSpan(0, 0, 0, 0, (int)result);
        }

        internal void PushAnswerTime(IPEndPoint host, TimeSpan duration)
        {
            if (!answerTimes.ContainsKey(host))
            {
                answerTimes[host] = new Queue<TimeSpan>();
            }

            Queue<TimeSpan> timeSpanQueue = answerTimes[host];
            timeSpanQueue.Enqueue(duration);
            if (timeSpanQueue.Count > 50)
            {
                timeSpanQueue.Dequeue();
            }
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
                sessionId = nextSessionId;
                nextSessionId++;
                nextSessionId &= 0x7FFFFFFF;
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
		
		public void Poll()
		{
			var handler = Received;
			if(handler)
			{
				foreach(ReceivingTransaction completedTransaction in completedTransactions)
				{
					handler(this, new NetworkEventArgs(completedTransaction.RemoteHost, completedTransaction.Data));
				}
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
