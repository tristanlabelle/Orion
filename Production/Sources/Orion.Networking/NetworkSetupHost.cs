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

        private void ProcessJoinRequest(IPEndPoint host)
        {
            Console.WriteLine("Received a join request from {0}", host);
            byte[] accept = new byte[1];
            accept[0] = (byte)SetupMessageType.AcceptJoinRequest;
            transporter.SendTo(accept, host);

            byte[] seeder = new byte[5];
            seeder[0] = (byte)SetupMessageType.Seed;
            BitConverter.GetBytes(seed).CopyTo(seeder, 1);
            transporter.SendTo(seeder, host);

            byte[] addPeerHostBytes = new byte[7];
            addPeerHostBytes[0] = (byte)SetupMessageType.AddPeer;
            host.CopyTo(addPeerHostBytes, 1);

            byte[] addPeerBytes = new byte[7];
            addPeerBytes[0] = (byte)SetupMessageType.AddPeer;
            foreach (IPEndPoint peer in peers)
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
        #endregion
    }
}
