using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Networking;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Event arguments for events raised when a join response was received.
    /// </summary>
    public struct JoinResponseEventArgs
    {
        #region Fields
        private readonly IPv4EndPoint endPoint;
        private readonly bool wasAccepted;
        #endregion

        #region Constructors
        public JoinResponseEventArgs(IPv4EndPoint endPoint, bool wasAccepted)
        {
            this.endPoint = endPoint;
            this.wasAccepted = wasAccepted;
        }
        #endregion

        #region Properties
        public IPv4EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public bool WasAccepted
        {
            get { return wasAccepted; }
        }
        #endregion

        #region Methods
        #endregion
    }
}
