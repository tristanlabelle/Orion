using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Orion.Networking
{
	/// <summary>
	/// A Transporter is responsible for safely transporting UDP packets over a network. It has the same guaratees the UDP protocol provides, plus
	/// it guarantees packets are going to be received.
	/// </summary>
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
        private readonly Thread senderThread;
        private readonly Thread receiverThread;
        private bool isDisposed;

        #endregion
        
        #region Public

		/// <summary>
		/// Holds the port to which the UDP socket is bound.
		/// </summary>
        public readonly int Port;

        #endregion
        #endregion

        #region Events

		/// <summary>
		/// The event triggered when a packet is successfully received.
		/// </summary>
        public event GenericEventHandler<Transporter, NetworkEventArgs> Received;

        #endregion

        #region Constructor

		/// <summary>
		/// Creates a Transporter, binding its UDP socket to the specified port.
		/// </summary>
		/// <param name="port">
		/// The port on which to bind
		/// </param>
        public Transporter(int port)
        {
            Port = port;
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			senderThread = new Thread(SenderThread);
			receiverThread = new Thread(ReceiverThread);
			
			senderThread.Start();
			receiverThread.Start();
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

                udpSocket.ReceiveFrom(buffer, ref endpointFrom);
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
				
				Transaction incomingTransaction;
				lock(hostTransactions)
				{
					if(!hostTransactions.ContainsKey(sessionId))
					{
						if((sessionId & localSessionMask) == localSessionMask)
						{
							throw new KeyNotFoundException(string.Format("Session id {0} should have been local but does not exist", sessionId));
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
            double result = 0;
			Queue<TimeSpan> timeSpans;
			lock(answerTimes)
			{
	            if (!answerTimes.ContainsKey(host))
	                return new TimeSpan(0, 0, 0, 100);
				timeSpans = answerTimes[host];
			}
			
			lock(timeSpans)
			{
	            foreach (TimeSpan answerTime in timeSpans)
	            {
	                result += answerTime.TotalMilliseconds;
	            }
			}
			
            return new TimeSpan(0, 0, 0, 0, (int)result);
        }

        internal void PushAnswerTime(IPEndPoint host, TimeSpan duration)
        {
			Queue<TimeSpan> timeSpans;
			lock(answerTimes)
			{
	            if (!answerTimes.ContainsKey(host))
	            {
	                answerTimes[host] = new Queue<TimeSpan>();
	            }
				timeSpans = answerTimes[host];
			}

			lock(timeSpans)
			{
	            timeSpans.Enqueue(duration);
	            if (timeSpans.Count > 50)
	            {
	                timeSpans.Dequeue();
	            }
			}
        }

        #endregion

        #region Public

		/// <summary>
		/// Sends data over the transporter to a specified address.
		/// </summary>
		/// <param name="data">
		/// The byte array to send
		/// </param>
		/// <param name="to">
		/// The destination <see cref="IPEndPoint"/>
		/// </param>
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

			Dictionary<uint, Transaction> hostTransactions;
            lock (transactions)
            {
                if (!transactions.ContainsKey(to))
                {
                    transactions[to] = new Dictionary<uint, Transaction>();
                }
				hostTransactions = transactions[to];
			}
			
			lock(hostTransactions)
			{
            		hostTransactions[sessionId] = transaction;
			}
        }
		
		/// <summary>
		/// Polls the Transporter, triggering reception events for every received packet.
		/// </summary>
		public void Poll()
		{
			CheckIfDisposed();
			var handler = Received;
			if(handler != null)
			{
				lock(completedTransactions)
				{
					foreach(ReceivingTransaction completedTransaction in completedTransactions)
					{
						handler(this, new NetworkEventArgs(completedTransaction.RemoteHost, completedTransaction.Data));
					}
				}
			}
		}

		/// <summary>
		/// Disposes of the Transporter.
		/// </summary>
        public void Dispose()
        {
            CheckIfDisposed();
            isDisposed = true;
			udpSocket.Shutdown(SocketShutdown.Both);
			udpSocket.Close();
        }

        #endregion

        #endregion
    }

}
