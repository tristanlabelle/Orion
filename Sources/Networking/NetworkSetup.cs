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
        protected List<IPv4EndPoint> peerEndPoints = new List<IPv4EndPoint>();
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
        public IEnumerable<IPv4EndPoint> PeerEndPoints
        {
            get { return peerEndPoints; }
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
            } while (peerEndPoints.Count == 0);
        }

        public void Dispose()
        {
            transporter.Received -= receptionDelegate;
        }
        #endregion
    }
}