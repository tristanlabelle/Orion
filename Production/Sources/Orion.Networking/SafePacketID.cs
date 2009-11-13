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
        #region Instance
        #region Fields
        private readonly IPv4EndPoint hostEndPoint;
        private readonly uint sessionID;
        #endregion

        #region Constructors
        public SafePacketID(IPv4EndPoint hostEndPoint, uint sessionID)
        {
            Argument.EnsureNotNull(hostEndPoint, "hostEndPoint");

            this.hostEndPoint = hostEndPoint;
            this.sessionID = sessionID;
        }
        #endregion

        #region Properties
        public IPv4EndPoint HostEndPoint
        {
            get { return hostEndPoint; }
        }

        public uint SessionID
        {
            get { return sessionID; }
        }
        #endregion

        #region Methods
        public bool Equals(SafePacketID other)
        {
            return hostEndPoint == other.hostEndPoint
                && sessionID == other.sessionID;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SafePacketID)) return false;
            return Equals((SafePacketID)obj);
        }

        public override int GetHashCode()
        {
            return (int)sessionID;
        }

        public override string ToString()
        {
            return "#{0} to host {1}".FormatInvariant(sessionID, hostEndPoint);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(SafePacketID a, SafePacketID b)
        {
            return a.Equals(b);
        }
        #endregion

        #region Operators
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(SafePacketID a, SafePacketID b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(SafePacketID a, SafePacketID b)
        {
            return !Equals(a, b);
        }
        #endregion
        #endregion
    }
}
