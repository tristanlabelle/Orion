using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    /// <summary>
    /// Uniquely identifies a packet of data sent to a specific remote host.
    /// </summary>
    internal struct PacketID
    {
        #region Fields
        private readonly IPEndPoint remoteHost;
        private readonly uint sessionId;
        #endregion

        #region Constructors
        public PacketID(IPEndPoint host, uint sessionID)
        {
            Argument.EnsureNotNull(host, "host");

            this.remoteHost = host;
            this.sessionId = sessionID;
        }
        #endregion

        #region Properties
        public IPEndPoint RemoteHost
        {
            get { return remoteHost; }
        }

        public uint SessionID
        {
            get { return sessionId; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "#{0} to host {1}".FormatInvariant(sessionId, remoteHost);
        }
        #endregion
    }
}
