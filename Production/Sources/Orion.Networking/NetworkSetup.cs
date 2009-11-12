using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Orion.Networking
{
    public abstract class NetworkSetup : IDisposable
    {
        #region Fields
        private GenericEventHandler<SafeTransporter, NetworkEventArgs> receptionDelegate;

        protected int seed;
        protected SafeTransporter transporter;
        protected List<IPEndPoint> peers = new List<IPEndPoint>();
        #endregion

        #region Constructors
        public NetworkSetup(SafeTransporter transporter)
        {
            Argument.EnsureNotNull(transporter, "transporter");
            this.transporter = transporter;

            receptionDelegate = new GenericEventHandler<SafeTransporter, NetworkEventArgs>(TransporterReceived);
            transporter.Received += receptionDelegate;
        }
        #endregion

        #region Properties
        public IEnumerable<IPEndPoint> Peers
        {
            get { return peers; }
        }
        #endregion

        #region Methods
        protected abstract void TransporterReceived(SafeTransporter source, NetworkEventArgs args);

        public void WaitForPeers()
        {
            // kludge to get just two players together
            // will eventually need to support more people :)
            do
            {
                Thread.Sleep(10);
                transporter.Poll();
            } while (peers.Count == 0);
        }

        public void Dispose()
        {
            transporter.Received -= receptionDelegate;
        }
        #endregion
    }
}