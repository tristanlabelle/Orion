using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Matchmaking.Networking.Packets;

namespace Orion.Game.Matchmaking.Networking
{
    public class LocalNetworkAdvertizer : IMatchAdvertizer
    {
        #region Fields
        private readonly GameNetworking networking;
        #endregion

        #region Constructors
        public LocalNetworkAdvertizer(GameNetworking networking)
        {
            this.networking = networking;
        }
        #endregion

        #region Methods
        public void Advertize(string name, int openSlotsCount)
        {
            AdvertizeMatchPacket advertize = new AdvertizeMatchPacket(name, openSlotsCount);
            networking.Broadcast(advertize);
        }
        #endregion
    }
}
