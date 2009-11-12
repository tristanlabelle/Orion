using System;
using System.Net;

namespace Orion.Networking
{
    public sealed class NetworkSetupHost : NetworkSetup
    {
        #region Constructors
        public NetworkSetupHost(SafeTransporter transporter)
            : base(transporter)
        {
            seed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
        #endregion

        #region Methods
        protected override void TransporterReceived(SafeTransporter source, NetworkEventArgs args)
        {
            switch ((SetupMessageType)args.Data[0])
            {
                case SetupMessageType.JoinRequest: ProcessJoinRequest(args.Host); return;
                case SetupMessageType.LeaveGame: ProcessLeaveGame(args.Host); return;
            }
        }

        private void ProcessJoinRequest(Ipv4EndPoint hostEndPoint)
        {
            Console.WriteLine("Received a join request from {0}", hostEndPoint);
            byte[] accept = new byte[1];
            accept[0] = (byte)SetupMessageType.AcceptJoinRequest;
            transporter.SendTo(accept, hostEndPoint);

            byte[] seeder = new byte[5];
            seeder[0] = (byte)SetupMessageType.Seed;
            BitConverter.GetBytes(seed).CopyTo(seeder, 1);
            transporter.SendTo(seeder, hostEndPoint);

            byte[] addPeerHostBytes = new byte[7];
            addPeerHostBytes[0] = (byte)SetupMessageType.AddPeer;
            hostEndPoint.CopyBytes(addPeerHostBytes, 1);

            byte[] addPeerBytes = new byte[7];
            addPeerBytes[0] = (byte)SetupMessageType.AddPeer;
            foreach (Ipv4EndPoint peerEndPoint in peerEndPoints)
            {
                peerEndPoint.CopyBytes(addPeerBytes, 1);

                transporter.SendTo(addPeerHostBytes, peerEndPoint);
                transporter.SendTo(addPeerBytes, hostEndPoint);
            }
            peerEndPoints.Add(hostEndPoint);
        }

        private void ProcessLeaveGame(Ipv4EndPoint host)
        {
            peerEndPoints.Remove(host);
        }
        #endregion
    }
}
