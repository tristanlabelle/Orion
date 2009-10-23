using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Networking;
using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
    class MultiplayerClientMatchConfigurer : MultiplayerMatchConfigurer
    {
        public MultiplayerClientMatchConfigurer(Transporter transporter)
            : base(transporter)
        { }

        public IPAddress Host { get; set; }

        public override void CreateNetworkConfiguration()
        {
            IPEndPoint admin = new IPEndPoint(Host, transporter.Port);
            using (NetworkSetupClient client = new NetworkSetupClient(transporter))
            {
                client.Join(admin);
                client.WaitForPeers();

                List<IPEndPoint> unsortedPeers = client.Peers.ToList();
                unsortedPeers.Sort(new Comparison<IPEndPoint>((a, b) =>
                    BitConverter.ToInt32(a.Address.GetAddressBytes(), 0) - BitConverter.ToInt32(b.Address.GetAddressBytes(), 0)));
                peers = unsortedPeers;
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
