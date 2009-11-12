using System;
using System.Net;

namespace Orion.Networking
{
    public sealed class NetworkSetupClient : NetworkSetup
    {
        #region Fields
        private Ipv4EndPoint server;
        #endregion

        #region Constructors
        public NetworkSetupClient(SafeTransporter transporter)
            : base(transporter) { }
        #endregion

        #region Methods
        public void Join(Ipv4EndPoint hostEndPoint)
        {
            Argument.EnsureNotNull(hostEndPoint, "host");
            Console.WriteLine("Asking {0} to join the game", hostEndPoint);

            byte[] packet = new byte[1];
            packet[0] = (byte)SetupMessageType.JoinRequest;
            transporter.SendTo(packet, hostEndPoint);
        }

        protected override void TransporterReceived(SafeTransporter source, NetworkEventArgs args)
        {
            switch ((SetupMessageType)args.Data[0])
            {
                case SetupMessageType.AcceptJoinRequest: ProcessAcceptJoinRequest(args.Host); return;
                case SetupMessageType.AddPeer: ProcessAddPeer(args.Data); return;
                case SetupMessageType.KickPeer: ProcessKickPeer(args.Host, args.Data); return;
                case SetupMessageType.LeaveGame: ProcessLeaveGame(args.Host); return;
                case SetupMessageType.RefuseJoinRequest: /* cry */ return;
                case SetupMessageType.Seed: seed = BitConverter.ToInt32(args.Data, 1); return;
            }
            throw new NotImplementedException("A network host should never get messages of setup type {0}".FormatInvariant(args.Data[0]));
        }

        private void ProcessAcceptJoinRequest(Ipv4EndPoint host)
        {
            Console.WriteLine("Server accepted our join request");
            server = host;
            AddPeer(host);
        }

        private void ProcessAddPeer(byte[] data)
        {
            AddPeer(Ipv4EndPoint.FromBytes(data, 1));
        }

        private void AddPeer(Ipv4EndPoint host)
        {
            if (!peerEndPoints.Contains(host))
            {
                peerEndPoints.Add(host);
            }
        }

        private void ProcessKickPeer(Ipv4EndPoint host, byte[] data)
        {
            if (server == host)
            {
                Ipv4EndPoint target = Ipv4EndPoint.FromBytes(data, 1);
                if (peerEndPoints.Contains(target))
                {
                    peerEndPoints.Remove(target);
                }
            }
        }

        private void ProcessLeaveGame(Ipv4EndPoint host)
        {
            if (peerEndPoints.Contains(host))
            {
                peerEndPoints.Remove(host);
            }
        }
        #endregion
    }
}
