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
    sealed class MultiplayerHostMatchConfigurer : MultiplayerMatchConfigurer
    {
        public MultiplayerHostMatchConfigurer(Transporter transporter)
            : base(transporter)
        {
            seed = 545845;
        }

        public override void CreateNetworkConfiguration()
        {
            using (NetworkSetupHost host = new NetworkSetupHost(transporter))
            {
                host.WaitForPeers();
                peers = host.Peers
                    .OrderBy(ipEndPoint => BitConverter.ToUInt32(ipEndPoint.Address.GetAddressBytes(), 0))
                    .ToList();
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
