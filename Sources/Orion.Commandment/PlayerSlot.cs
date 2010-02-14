using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using System.Net;

namespace Orion.Commandment
{
    public abstract class PlayerSlot
    {
        public abstract bool NeedsFaction { get; }
        public abstract override string ToString();
    }

    public sealed class RemotePlayerSlot : PlayerSlot
    {
        private IPv4EndPoint? remoteHost;
        private string name;

        public IPv4EndPoint? RemoteHost
        {
            get { return remoteHost; }
            set
            {
                remoteHost = value;
                if (remoteHost.HasValue)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(remoteHost.Value.Address);
                    name = hostEntry == null ? null : hostEntry.HostName;
                }
                else
                {
                    name = null;
                }
            }
        }

        public override bool NeedsFaction
        {
            get { return RemoteHost.HasValue; }
        }

        public override string ToString()
        {
            if (RemoteHost.HasValue)
                return name == null ? RemoteHost.Value.ToString() : name;
            return "Ouvert";
        }
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
