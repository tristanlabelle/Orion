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
    class MultiplayerClientMatchConfigurer : MultiplayerMatchConfigurer
    {
        public MultiplayerClientMatchConfigurer(SafeTransporter transporter)
            : base(transporter)
        {
            seed = 545845;
        }

        public IPEndPoint Host { get; set; }

        public override void CreateNetworkConfiguration()
        {
            using (NetworkSetupClient client = new NetworkSetupClient(transporter))
            {
                client.Join(Host);
                client.WaitForPeers();
                peers = client.Peers
                    .OrderBy(ipEndPoint => BitConverter.ToUInt32(ipEndPoint.Address.GetAddressBytes(), 0))
                    .ToList();
            }
        }

        protected override void AssignFactions(out UserInputCommander userCommander)
        {
            world.CreateFaction("Red", Color.Red);
            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            userCommander = new UserInputCommander(blueFaction);
        }
    }
}
