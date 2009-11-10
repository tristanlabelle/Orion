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
    internal struct SafePacketID : IEquatable<SafePacketID>
    {
        #region Fields
        private readonly IPEndPoint remoteHost;
        private readonly uint sessionID;
        #endregion

        #region Constructors
        public SafePacketID(IPEndPoint host, uint sessionID)
        {
            Argument.EnsureNotNull(host, "host");

            this.remoteHost = host;
            this.sessionID = sessionID;
        }
        #endregion

        #region Properties
        public IPEndPoint RemoteHost
        {
            get { return remoteHost; }
        }

        public uint SessionID
        {
            get { return sessionID; }
        }
        #endregion

        #region Methods
        public bool Equals(SafePacketID other)
        {
            return other.remoteHost == remoteHost && other.sessionID == sessionID;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SafePacketID)) return false;
            return Equals((SafePacketID)obj);
        }

        public override string ToString()
        {
            return "#{0} to host {1}".FormatInvariant(sessionID, remoteHost);
        }
        #endregion
    }
}
