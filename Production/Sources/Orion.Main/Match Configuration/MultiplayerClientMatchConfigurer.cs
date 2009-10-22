using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;

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
    }
}
