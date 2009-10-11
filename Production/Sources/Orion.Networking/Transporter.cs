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
	public class Transporter
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
		private const byte DataPacket = 0;
		private const byte AcknowledgePacket = 1;
		private const long DefaultPing = 100;
		
		private uint nextSessionId;
		
		private readonly Queue<NetworkEventArgs> readyData = new Queue<NetworkEventArgs>();
		
		private readonly Dictionary<IPEndPoint, Queue<long>> pings = new Dictionary<IPEndPoint, Queue<long>>();
		private readonly List<PacketId> answeredPackets = new List<PacketId>();
		private readonly List<PacketId> packetsToAnswer = new List<PacketId>();
		
		private readonly Dictionary<PacketId, PacketSession> packetsToSend = new Dictionary<PacketId, PacketSession>();
		private readonly Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		
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
			udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
			senderThread = new Thread(SenderThread);
			receiverThread = new Thread(ReceiverThread);
		}
		#endregion
		
		#region Methods
		
		private void ValidateObjectState()
		{
			if(isDisposed) throw new ObjectDisposedException(null);
		}
		
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
					deviation += Math.Sqrt(pingDeviation * pingDeviation);
				}
				deviation /= hostPings.Count();
			}
			
			return deviation;
		}
		
		private void ReceiverThread()
		{
			byte[] answer = new byte[5];
			answer[0] = AcknowledgePacket;
			
			byte[] packet = new byte[1024];
			EndPoint endpoint = new IPEndPoint(0, 0);
			while(true)
			{
				if(isDisposed) break;
				
				lock(udpSocket)
				{
					try
					{
						udpSocket.ReceiveFrom(packet, ref endpoint);
					}
					catch(SocketException e)
					{
						Console.WriteLine("Socket threw up a SocketException with code {0}", e.ErrorCode);
						break;
					}
				}
				
				uint sessionId = BitConverter.ToUInt32(packet, 1);
				PacketId id = new PacketId(sessionId, endpoint as IPEndPoint);
				
				if(packet[0] == DataPacket)
				{
					lock(answeredPackets)
					{
						if(answeredPackets.Contains(id) || packetsToAnswer.Contains(id)) continue;
						answeredPackets.Add(id);
					}
					
					lock(readyData)
					{
						readyData.Enqueue(new NetworkEventArgs(id.RemoteHost, packet.Skip(5).ToArray()));
					}
					
					// copy the session id
					for(int i = 1; i < 5; i++)
						answer[i] = packet[i];
					
					lock(udpSocket)
					{
						udpSocket.SendTo(answer, endpoint);
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
		
		private void SenderThread()
		{
			while(true)
			{
				if(isDisposed) break;
				
				List<PacketSession> sessions;
				lock(packetsToSend)
				{
					sessions = packetsToSend.Values.ToList();
				}
				
				foreach(PacketSession session in sessions)
				{
					if(session.NeedsResend)
					{
						lock(udpSocket)
						{
							session.SendThrough(udpSocket);
							session.ResetSendTime(AveragePing(session.Id.RemoteHost) + StandardDeviationForPings(session.Id.RemoteHost));
						}
					}
				}
				
				Thread.Sleep(10);
			}
		}
		
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
		
		public void Dispose()
		{
			isDisposed = true;
			udpSocket.Shutdown(SocketShutdown.Both);
			udpSocket.Close();
		}
		
		#endregion
	}
}
