using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;

namespace Orion.Networking
{
    public class CommandReceiver : IDisposable
    {
        #region Fields
        private GenericEventHandler<SafeTransporter, NetworkEventArgs> receive;
        private GenericEventHandler<SafeTransporter, IPv4EndPoint> timeout;
        private readonly SafeTransporter transporter;

        public readonly Faction Faction;
        public readonly IPv4EndPoint Host;
        #endregion

        #region Constructors
        public CommandReceiver(SafeTransporter transporter, Faction faction, IPv4EndPoint host)
        {
            receive = OnReceived;
            timeout = OnTimedOut;
            this.transporter = transporter;
            Faction = faction;
            Host = host;
        }
        #endregion

        #region Methods
        private void OnReceived(SafeTransporter transporter, NetworkEventArgs args)
        {
            if (args.Host != Host) return;
        }

        private void OnTimedOut(SafeTransporter transporter, IPv4EndPoint endPoint)
        {
            if (endPoint != Host) return;
        }

        public void Dispose()
        {

        }
        #endregion
    }
}
