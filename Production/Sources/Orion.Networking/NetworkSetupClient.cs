using System;
using System.Net;

namespace Orion.Networking
{
	public class NetworkSetupClient : NetworkSetupHelper
	{
		private IPEndPoint server;
		
		public NetworkSetupClient(Transporter transporter)
			: base(transporter)
		{ }
		
		public void Join(IPEndPoint host)
		{
			byte[] packet = new byte[1];
			packet[0] = (byte)SetupMessageType.JoinRequest;
			transporter.SendTo(packet, host);
		}
		
		protected override void TransporterReceived(Transporter source, NetworkEventArgs args)
		{
			switch((SetupMessageType)args.Data[0])
			{
				case SetupMessageType.AcceptJoinRequest: ProcessAcceptJoinRequest(args.Host); return;
				case SetupMessageType.AddPeer: ProcessAddPeer(args.Data); return;
				case SetupMessageType.KickPeer: ProcessKickPeer(args.Host, args.Data); return;
				case SetupMessageType.LeaveGame: ProcessLeaveGame(args.Host); return;
				case SetupMessageType.RefuseJoinRequest: /* cry */ return;
			}
			throw new NotImplementedException("A network host should never get messages of setup type {0}".FormatInvariant(args.Data[0]));
		}
		
		private void ProcessAcceptJoinRequest(IPEndPoint host)
		{
			server = host;
			AddPeer(host);
		}
		
		private void ProcessAddPeer(byte[] data)
		{
			AddPeer(IPEndPointSerialization.Unserialize(data, 1));
		}
		
		private void AddPeer(IPEndPoint host)
		{
			if(!peers.Contains(host))
			{
				peers.Add(host);
			}
		}
		
		private void ProcessKickPeer(IPEndPoint host, byte[] data)
		{
			if(server == host)
			{
				IPEndPoint target = IPEndPointSerialization.Unserialize(data, 1);
				if(peers.Contains(target))
				{
					peers.Remove(target);
				}
			}
		}
		
		private void ProcessLeaveGame(IPEndPoint host)
		{
			if(peers.Contains(host))
			{
				peers.Remove(host);
			}
		}
	}
}