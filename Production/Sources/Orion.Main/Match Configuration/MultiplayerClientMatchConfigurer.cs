using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Networking;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    sealed class MultiplayerClientMatchConfigurer : MultiplayerMatchConfigurer
    {
        #region Constructors
        public MultiplayerClientMatchConfigurer(SafeTransporter transporter)
            : base(transporter)
        {
            seed = 545845;
        }
        #endregion

        #region Properties
        public IPv4EndPoint HostEndPoint { get; set; }
        #endregion

        #region Methods
        public override void CreateNetworkConfiguration()
        {
            using (NetworkSetupClient client = new NetworkSetupClient(transporter))
            {
                client.Join(HostEndPoint);
                client.WaitForPeers();
                base.peerEndPoints = client.PeerEndPoints
                    .OrderBy(endPoint => endPoint)
                    .ToList();
            }
        }

        protected override void AssignFactions(out UserInputCommander userCommander)
        {
            world.CreateFaction("Red", Color.Red);
            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            userCommander = new UserInputCommander(blueFaction);
        }
        #endregion
    }
}
