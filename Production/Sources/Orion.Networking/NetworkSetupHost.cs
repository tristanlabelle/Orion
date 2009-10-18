using System;
using System.Net;

namespace Orion.Networking
{
	public class NetworkSetupHost : NetworkSetupHelper
	{
		public NetworkSetupHost(Transporter transporter)
			: base(transporter)
		{ }
		
		protected override void TransporterReceived(Transporter source, NetworkEventArgs args)
		{
			switch((SetupMessageType)args.Data[0])
			{
				case SetupMessageType.JoinRequest: ProcessJoinRequest(args.Host); return;
				case SetupMessageType.LeaveGame: ProcessLeaveGame(args.Host); return;
			}
			throw new NotImplementedException("A network host should never get messages of setup type {0}".FormatInvariant(args.Data[0]));
		}
		
		private void ProcessJoinRequest(IPEndPoint host)
		{
			byte[] accept = new byte[1];
			accept[0] = (byte)SetupMessageType.AcceptJoinRequest;
			transporter.SendTo(accept, host);
			
			byte[] addPeerHostBytes = new byte[7];
			addPeerHostBytes[0] = (byte)SetupMessageType.AddPeer;
			host.CopyTo(addPeerHostBytes, 1);
			
			byte[] addPeerBytes = new byte[7];
			addPeerBytes[0] = (byte)SetupMessageType.AddPeer;
			foreach(IPEndPoint peer in peers)
			{
				peer.CopyTo(addPeerBytes, 1);
				
				transporter.SendTo(addPeerHostBytes, peer);
				transporter.SendTo(addPeerBytes, host);
			}
			peers.Add(host);
		}
		
		private void ProcessLeaveGame(IPEndPoint host)
		{
			peers.Remove(host);
		}
	}
}