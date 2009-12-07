using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents what is remembered of a building which has been seen but is now in the fog of war.
    /// </summary>
    public struct RememberedBuilding : IEquatable<RememberedBuilding>
    {
        #region Instance
        #region Fields
        private readonly Point location;
        private readonly UnitType type;
        private readonly Faction faction;
        #endregion

        #region Constructors
        public RememberedBuilding(Unit building)
        {
            Argument.EnsureNotNull(building, "building");
            Debug.Assert(building.IsBuilding);

            this.location = building.GridRegion.Min;
            this.type = building.Type;
            this.faction = building.Faction;
        }
        #endregion

        #region Properties
        public Point Location
        {
            get { return location; }
        }

        public Region GridRegion
        {
            get { return Entity.GetGridRegion(location, type.Size); }
        }

        public UnitType Type
        {
            get { return type; }
        }

        public Faction Faction
        {
            get { return faction; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if a building matches this description.
        /// </summary>
        /// <param name="building">The building to be tested.</param>
        /// <returns>A value indicating if it matches this description.</returns>
        public bool Matches(Unit building)
        {
            Argument.EnsureNotNull(building, "building");
            return location == building.GridRegion.Min
                && type == building.Type
                && faction == building.Faction;
        }

        public bool Equals(RememberedBuilding other)
        {
            return location == other.location
                && type == other.type
                && faction == other.faction;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RememberedBuilding)) return false;
            return Equals((RememberedBuilding)obj);
        }

        public override int GetHashCode()
        {
            return location.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} {1} at {2}".FormatInvariant(faction, type, location);
        }
        #endregion
        #endregion

        #region Static
        public static bool Equals(RememberedBuilding a, RememberedBuilding b)
        {
            return a.Equals(b);
        }

        public static bool operator ==(RememberedBuilding a, RememberedBuilding b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(RememberedBuilding a, RememberedBuilding b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}
