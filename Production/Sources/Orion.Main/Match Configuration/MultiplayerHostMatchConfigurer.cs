using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Color = System.Drawing.Color;

using Orion.Networking;
using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
    sealed class MultiplayerHostMatchConfigurer : MultiplayerMatchConfigurer
    {
        public MultiplayerHostMatchConfigurer(Transporter transporter)
            : base(transporter)
        { }

        public override void CreateNetworkConfiguration()
        {
            using (NetworkSetupHost host = new NetworkSetupHost(transporter))
            {
                host.WaitForPeers();

                List<IPEndPoint> unsortedPeers = host.Peers.ToList();

                unsortedPeers.Sort(new Comparison<IPEndPoint>((a, b) =>
                    BitConverter.ToInt32(a.Address.GetAddressBytes(), 0) - BitConverter.ToInt32(b.Address.GetAddressBytes(), 0)));
                peers = unsortedPeers;
            }
        }

        protected override void AssignFactions(out UserInputCommander userCommander)
        {
            Faction redFaction = world.CreateFaction("Red", Color.Red);
            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            userCommander = new UserInputCommander(redFaction);
        }
    }
}
