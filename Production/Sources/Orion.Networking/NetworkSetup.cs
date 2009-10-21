using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Networking
{
	public enum SetupMessageType : byte
	{
		JoinRequest,
		AcceptJoinRequest,
		RefuseJoinRequest,
		AddPeer,
		KickPeer,
		LeaveGame
	}
	
	public abstract class NetworkSetupHelper : IDisposable
	{
		private GenericEventHandler<Transporter, NetworkEventArgs> receptionDelegate;
		
		protected Transporter transporter;
		protected List<IPEndPoint> peers;
		
		public NetworkSetupHelper(Transporter transporter)
		{
			Argument.EnsureNotNull(transporter, "transporter");
			this.transporter = transporter;
			peers = new List<IPEndPoint>();
			
			receptionDelegate = new GenericEventHandler<Transporter, NetworkEventArgs>(TransporterReceived);
			transporter.Received += receptionDelegate;
		}
		
		public IEnumerable<IPEndPoint> Peers
		{
			get { return peers; }
		}
		
		protected abstract void TransporterReceived(Transporter source, NetworkEventArgs args);
		
		public void WaitForPeers()
		{
			// kludge to get just two players together
			// will eventually need to support more people :)
			do
			{
				Thread.Sleep(10);
				transporter.Poll();
			} while(peers.Count == 0);
		}
		
		public void Dispose()
		{
			transporter.Received -= receptionDelegate;
		}
	}
	
	internal static class IPEndPointSerialization
	{
		public static void CopyTo(this IPEndPoint endpoint, byte[] array, long index)
		{
			byte[] addressBytes = endpoint.Address.GetAddressBytes();
			addressBytes.CopyTo(array, index);
			ushort port = (ushort)endpoint.Port;
			array[index + addressBytes.Length] = (byte)(port & 0xFF);
			array[index + addressBytes.Length + 1] = (byte)(port >> 8);
		}
		
		public static IPEndPoint Unserialize(byte[] array, int index)
		{
			long address = BitConverter.ToInt32(array, index);
			int port = BitConverter.ToUInt16(array, index + 4);
			return new IPEndPoint(new IPAddress(address), port);
		}
	}
}
