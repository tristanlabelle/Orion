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
        public MultiplayerClientMatchConfigurer(Transporter transporter)
            : base(transporter)
        {
            seed = 545845;
        }

        public IPAddress Host { get; set; }

        public override void CreateNetworkConfiguration()
        {
            IPEndPoint admin = new IPEndPoint(Host, Program.DefaultPort);
            using (NetworkSetupClient client = new NetworkSetupClient(transporter))
            {
                client.Join(admin);
                client.WaitForPeers();
                peers = client.Peers
                    .OrderBy(ipEndPoint => BitConverter.ToUInt32(ipEndPoint.Address.GetAddressBytes(), 0))
                    .ToList();
            }
        }

        protected override void AssignFactions(out UserInputCommander userCommander)
        {
            Faction redFaction = world.CreateFaction("Red", Color.Red);
            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            userCommander = new UserInputCommander(blueFaction);
        }
    }
}
