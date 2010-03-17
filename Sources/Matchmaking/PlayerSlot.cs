using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Orion.Engine.Networking;
using Orion.GameLogic;

namespace Orion.Matchmaking
{
    public abstract class PlayerSlot
    {
        public abstract bool NeedsFaction { get; }
        public abstract override string ToString();
    }

    public sealed class RemotePlayerSlot : PlayerSlot
    {
        #region Fields
        private IPv4EndPoint? hostEndPoint;
        private string hostName;
        #endregion

        #region Properties
        public IPv4EndPoint? HostEndPoint
        {
            get { return hostEndPoint; }
            set
            {
                hostEndPoint = value;
                if (hostEndPoint.HasValue)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(hostEndPoint.Value.Address);
                    hostName = hostEntry == null ? null : hostEntry.HostName;
                }
                else
                {
                    hostName = null;
                }
            }
        }

        public string HostName
        {
            get { return hostName; }
        }

        public override bool NeedsFaction
        {
            get { return HostEndPoint.HasValue; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            if (!hostEndPoint.HasValue) return "Ouvert";
            return hostName == null ? hostEndPoint.Value.ToString() : hostName;
        }
        #endregion
    }

    public sealed class ClosedPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return false; }
        }

        public override string ToString()
        {
            return "Fermé";
        }
    }

    public sealed class AIPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return true; }
        }

        public override string ToString()
        {
            return "Ordinateur";
        }
    }

    public sealed class LocalPlayerSlot : PlayerSlot
    {
        public override bool NeedsFaction
        {
            get { return true; }
        }

        public override string ToString()
        {
            return "Vous";
        }
    }
}
