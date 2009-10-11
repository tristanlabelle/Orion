using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

using Orion.Core;

namespace Orion.Networking
{
	/// <summary>
	/// A Transporter is responsible for safely transporting UDP packets over a network. It guarantees that packets are going to arrive, and that they will have
	/// been properly transported. The only guarantee not provided is the order in which they arrive.
	/// It creates a single UDP socket for communication to various hosts. The remote host must use a Transporter as well for reception.
	/// </summary>
	public sealed class Transporter : IDisposable
	{
		#region Nested Types
		
		private struct PacketId
		{
			public readonly IPEndPoint RemoteHost;
			public readonly uint SessionId;
			
			public PacketId(uint sid, IPEndPoint host)
			{
				RemoteHost = host;
				SessionId = sid;
			}
		}
		
		private struct PacketData
		{
			public readonly PacketId Id;
			public readonly byte[] Data;
			
			public PacketData(PacketId id, byte[] data)
			{
				Id = id;
				Data = data;
			}
		}
		
		private class PacketSession
		{
			private static readonly TimeSpan expiration = new TimeSpan(0, 0, 5);
			
			private readonly DateTime creationTime;
			private Stopwatch timeToReceive;
			private DateTime whenToResend;
			private byte[] fullPacket;
			
			public readonly PacketId Id;
			public readonly byte[] Data;
			
			public PacketSession(PacketId id, byte[] data)
			{
				timeToReceive = new Stopwatch();
				creationTime = DateTime.UtcNow;
				whenToResend = creationTime;
				Id = id;
				Data = data;
				
				fullPacket = new byte[data.Length + 5];
				fullPacket[0] = DataPacket;
				BitConverter.GetBytes(id.SessionId).CopyTo(fullPacket, 1);
				Data.CopyTo(fullPacket, 5);
			}
			
			public void SendThrough(Socket udpSocket)
			{
				timeToReceive.Reset();
				udpSocket.SendTo(fullPacket, Id.RemoteHost);
			}
			
			public void ResetSendTime(long milliseconds)
			{
				whenToResend = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, (int)milliseconds);
			}
			
			public bool NeedsResend
			{
				get { return DateTime.UtcNow >= whenToResend; }
			}
			
			public bool HasTimedOut
			{
				get { return whenToResend - creationTime > expiration; }
			}
			
			public long Acknowledge()
			{
				timeToReceive.Stop();
				return timeToReceive.ElapsedMilliseconds;
			}
		}
		
		#endregion
		
		#region Fields
		#region Private
		#region Constants
		private const byte DataPacket = 0;
		private const byte AcknowledgePacket = 1;
		private const long DefaultPing = 100;
		#endregion
		
		private uint nextSessionId;
		
		private readonly Queue<NetworkEventArgs> readyData = new Queue<NetworkEventArgs>();
		private readonly Queue<NetworkTimeoutEventArgs> timedOut = new Queue<NetworkTimeoutEventArgs>();
		
		private readonly Dictionary<IPEndPoint, Queue<long>> pings = new Dictionary<IPEndPoint, Queue<long>>();
		private readonly List<PacketId> answeredPackets = new List<PacketId>();
		
		private readonly Dictionary<PacketId, PacketSession> packetsToSend = new Dictionary<PacketId, PacketSession>();
		
		private readonly Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		
		private readonly Thread senderThread;
		private readonly Thread receiverThread;
		
		private bool isDisposed;
		
		#endregion
		
		#region Public
		/// <summary>
		/// The port on which the UDP socket is bound.
		/// </summary>
		public readonly int Port;
		#endregion
		#endregion
		
		#region Events
		
		/// <summary>
		/// The event triggered when a packet arrives.
		/// </summary>
		/// <remarks>This event is only triggered when the method <see cref="M:Poll"/> is called.</remarks>
		public event GenericEventHandler<Transporter, NetworkEventArgs> Received;
		
		/// <summary>
		/// The event triggered when a packet cannot reach its destination.
		/// </summary>
		/// <remarks>This event is only triggered when the method <see cref="M:Poll"/> is called.</remarks>
		public event GenericEventHandler<Transporter, NetworkTimeoutEventArgs> TimedOut;
		
		#endregion
		
		#region Constructor
		/// <summary>
		/// Creates a new Transporter whose UDP socket is bound to a specified port on all interfaces.
		/// </summary>
		/// <param name="port">
		/// The port on which to bind
		/// </param>
		public Transporter(int port)
		{
			Port = port;
			udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
			senderThread = new Thread(SenderThread);
			receiverThread = new Thread(ReceiverThread);
			
			receiverThread.Start();
			senderThread.Start();
		}
		#endregion
		
		#region Methods
		
		#region Private Methods
		private void ValidateObjectState()
		{
			if(isDisposed) throw new ObjectDisposedException(null);
		}
		
		#region Ping calculation methods
		private void AddPing(IPEndPoint host, long milliseconds)
		{
			Queue<long> hostPings;
			lock(pings)
			{
				if(!pings.ContainsKey(host))
				{
					pings[host] = new Queue<long>();
				}
				hostPings = pings[host];
			}
			
			lock(hostPings)
			{
				hostPings.Enqueue(milliseconds);
				if(hostPings.Count > 50)
				{
					hostPings.Dequeue();
				}
			}
		}
		
		private long AveragePing(IPEndPoint host)
		{
			Queue<long> hostPings;
			lock(pings)
			{
				if(!pings.ContainsKey(host))
				{
					pings[host] = new Queue<long>();
				}
				hostPings = pings[host];
			}
			
			lock(hostPings)
			{
				if(hostPings.Count == 0)
					return DefaultPing;
				return (long)hostPings.Average();
			}
		}
		
		private long StandardDeviationForPings(IPEndPoint host)
		{
			long average = AveragePing(host);
			
			long deviation = 0;
			Queue<long> hostPings;
			lock(pings)
			{
				hostPings = pings[host];
			}
			
			lock(hostPings)
			{
				foreach(long ping in hostPings)
				{
					long pingDeviation = average - ping;
					deviation += (long)Math.Sqrt(pingDeviation * pingDeviation);
				}
				deviation /= hostPings.Count();
			}
			
			return deviation;
		}
		
		#endregion
		
		#region Threading methods
		
		private void ReceiverThread()
		{
			byte[] answer = new byte[5];
			answer[0] = AcknowledgePacket;
			
			byte[] packet = new byte[1024];
			EndPoint endpoint = new IPEndPoint(0, 0);
			
			try
			{
				while(true)
				{
					if(isDisposed) break;
					
					lock(udpSocket)
					{
						udpSocket.ReceiveFrom(packet, ref endpoint);
					}
					
					uint sessionId = BitConverter.ToUInt32(packet, 1);
					PacketId id = new PacketId(sessionId, endpoint as IPEndPoint);
					
					if(packet[0] == DataPacket)
					{
						// copy the session id
						for(int i = 1; i < 5; i++)
							answer[i] = packet[i];
						
						// it is always necessary to send an answer to data packets
						lock(udpSocket)
						{
							udpSocket.SendTo(answer, endpoint);
						}
						
						lock(answeredPackets)
						{
							if(answeredPackets.Contains(id)) continue;
							answeredPackets.Add(id);
						}
						
						lock(readyData)
						{
							readyData.Enqueue(new NetworkEventArgs(id.RemoteHost, packet.Skip(5).ToArray()));
						}
					}
					else
					{
						lock(packetsToSend)
						{
							if(packetsToSend.ContainsKey(id))
							{
								AddPing(id.RemoteHost, packetsToSend[id].Acknowledge());
								packetsToSend.Remove(id);
							}
						}
					}
				}
			}
			catch(SocketException e)
			{
				Console.WriteLine("Broke from socket exception {0}", e.ErrorCode);
			}
		}
		
		private void SenderThread()
		{
			try
			{
				List<PacketSession> trash = new List<PacketSession>();
				while(true)
				{
					if(isDisposed) break;
					
					List<PacketSession> sessions;
					lock(packetsToSend)
					{
						sessions = packetsToSend.Values.ToList();
					}
					
					lock(udpSocket)
					{
						foreach(PacketSession session in sessions)
						{
							if(session.HasTimedOut)
							{
								trash.Add(session);
								timedOut.Enqueue(new NetworkTimeoutEventArgs(session.Id.RemoteHost, session.Data));
							}
							
							if(session.NeedsResend)
							{
								session.SendThrough(udpSocket);
								session.ResetSendTime(AveragePing(session.Id.RemoteHost) + StandardDeviationForPings(session.Id.RemoteHost));
							}
						}
					}
					
					lock(packetsToSend)
					{
						foreach(PacketSession session in trash)
						{
							packetsToSend.Remove(session.Id);
						}
					}
					
					Thread.Sleep(10);
				}
			}
			catch(SocketException e)
			{
				Console.WriteLine("Broke from socket exception {0}", e.ErrorCode);
			}
		}
		
		#endregion
		
		#endregion
		
		#region Public methods
		
		/// <summary>
		/// Sends data to a specified host.
		/// </summary>
		/// <param name="data">
		/// The data to send
		/// </param>
		/// <param name="remoteAddress">
		/// The host to which the data is addressed
		/// </param>
		public void SendTo(byte[] data, IPEndPoint remoteAddress)
		{
			ValidateObjectState();
			
			uint sid = nextSessionId;
			nextSessionId++;
			byte[] packet = new byte[data.Length + 4];
			BitConverter.GetBytes(sid).CopyTo(packet, 0);
			data.CopyTo(packet, sizeof(uint));
			
			lock(packetsToSend)
			{
				PacketId id = new PacketId(sid, remoteAddress);
				packetsToSend[id] = new PacketSession(id, packet);
			}
		}
		
		/// <summary>
		/// Triggers any pending packet reception event.
		/// </summary>
		/// <remarks>Events are not triggered until you call this method.</remarks>
		public void Poll()
		{
			ValidateObjectState();
			
			GenericEventHandler<Transporter, NetworkEventArgs> receptionHandler = Received;
			if(receptionHandler != null)
			{
				lock(readyData)
				{
					while(readyData.Count() > 0)
					{
						receptionHandler(this, readyData.Dequeue());
					}
				}
			}
			
			GenericEventHandler<Transporter, NetworkTimeoutEventArgs> timeoutHandler = TimedOut;
			if(timeoutHandler != null)
			{
				lock(timedOut)
				{
					while(timedOut.Count() > 0)
					{
						timeoutHandler(this, timedOut.Dequeue());
					}
				}
			}
		}
		
		/// <summary>
		/// Disposes of this object.
		/// </summary>
		public void Dispose()
		{
			isDisposed = true;
			lock(udpSocket)
			{
				udpSocket.Shutdown(SocketShutdown.Both);
				udpSocket.Close();
			}
		}
		
		#endregion
		
		#endregion
	}
}
