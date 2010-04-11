using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Networking;
using Orion.Engine;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// Represents a match that was advertized in the lobby.
    /// </summary>
    public sealed class AdvertizedMatch
    {
        #region Fields
        private readonly IPv4EndPoint endPoint;
        private string name;
        private int openSlotCount;
        private DateTime lastUpdateTime = DateTime.Now;
        #endregion

        #region Constructors
        public AdvertizedMatch(IPv4EndPoint endPoint, string name, int openSlotCount)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsurePositive(openSlotCount, "openSlotCount");

            this.endPoint = endPoint;
            this.name = name;
            this.openSlotCount = openSlotCount;
        }
        #endregion

        #region Properties
        public IPv4EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                this.name = value;
            }
        }

        public int OpenSlotCount
        {
            get { return openSlotCount; }
            set
            {
                Argument.EnsurePositive(value, "OpenSlotCount");
                this.openSlotCount = value;
            }
        }

        public DateTime LastUpdateTime
        {
            get { return lastUpdateTime; }
        }

        public TimeSpan TimeSinceLastUpdated
        {
            get { return DateTime.Now - lastUpdateTime; }
        }
        #endregion

        #region Methods
        public void KeepAlive()
        {
            lastUpdateTime = DateTime.Now;
        }
        #endregion
    }
}
