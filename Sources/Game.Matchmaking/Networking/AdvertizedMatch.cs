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
    public sealed class AdvertizedMatch : IEquatable<AdvertizedMatch>
    {
        #region Fields
        private readonly IMatchQuerier source;
        private readonly IPv4EndPoint endPoint;
        private string name;
        private int openSlotCount;
        private DateTime lastUpdateTime = DateTime.Now;
        #endregion

        #region Constructors
        public AdvertizedMatch(IMatchQuerier source, IPv4EndPoint endPoint, string name, int openSlotCount)
        {
            Argument.EnsureNotNull(source, "source");
            Argument.EnsureNotNull(name, "name");
            Argument.EnsurePositive(openSlotCount, "openSlotCount");

            this.source = source;
            this.endPoint = endPoint;
            this.name = name;
            this.openSlotCount = openSlotCount;
        }
        #endregion

        #region Properties
        public IMatchQuerier Source
        {
            get { return source; }
        }

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
        public bool Equals(AdvertizedMatch other)
        {
        	if (ReferenceEquals(other, null)) return false;
        	return name == other.name && openSlotCount == other.openSlotCount;
        }
        
		public override bool Equals(object obj)
		{
			return Equals(obj as AdvertizedMatch);
		}
		
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
		
		public override string ToString()
		{
			return "{0} ({1} places)".FormatInvariant(name, openSlotCount);
		}
        
        public void KeepAlive()
        {
            lastUpdateTime = DateTime.Now;
        }
        
        public static bool Equals(AdvertizedMatch a, AdvertizedMatch b)
        {
            if (object.ReferenceEquals(a, null)) return object.ReferenceEquals(b, null);
            return a.Equals(b);
        }
        
        public static bool operator ==(AdvertizedMatch a, AdvertizedMatch b)
        {
        	return Equals(a, b);
        }

        public static bool operator !=(AdvertizedMatch a, AdvertizedMatch b)
        {
        	return !Equals(a, b);
        }
        #endregion
    }
}
