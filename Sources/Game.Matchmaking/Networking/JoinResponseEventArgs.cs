using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Event arguments for events raised when a join response was received.
    /// </summary>
    public struct JoinResponseEventArgs
    {
        #region Fields
        private readonly IPv4EndPoint hostEndPoint;
        private readonly bool wasAccepted;
        #endregion

        #region Constructors
        public JoinResponseEventArgs(IPv4EndPoint hostEndPoint, bool wasAccepted)
        {
            this.hostEndPoint = hostEndPoint;
            this.wasAccepted = wasAccepted;
        }
        #endregion

        #region Properties
        public IPv4EndPoint HostEndPoint
        {
            get { return hostEndPoint; }
        }

        public bool WasAccepted
        {
            get { return wasAccepted; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Host {0} {1} the join request."
                .FormatInvariant(hostEndPoint, wasAccepted ? "accepted" : "rejected");
        }
        #endregion
    }
}
