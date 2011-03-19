using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.Utilities
{
    /// <summary>
    /// Represents what is remembered of a building which has been seen but is now in the fog of war.
    /// </summary>
    [ImmutableObject(true)]
    public struct RememberedBuilding : IEquatable<RememberedBuilding>
    {
        #region Instance
        #region Fields
        private readonly Point location;
        private readonly Entity prototype;
        private readonly Faction faction;
        #endregion

        #region Constructors
        public RememberedBuilding(Entity building)
        {
            Argument.EnsureNotNull(building, "building");

            this.location = building.GridRegion.Min;
            this.prototype = Identity.GetPrototype(building);
            this.faction = FactionMembership.GetFaction(building);
        }
        #endregion

        #region Properties
        public Point Location
        {
            get { return location; }
        }

        public Region GridRegion
        {
            get { return Entity.GetGridRegion(location, prototype.Size); }
        }

        public Entity Prototype
        {
            get { return prototype; }
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

            return new RememberedBuilding(building) == this;
        }

        public bool Equals(RememberedBuilding other)
        {
            return location == other.location
                && prototype == other.prototype
                && faction == other.faction;
        }

        public override bool Equals(object obj)
        {
            return obj is RememberedBuilding && Equals((RememberedBuilding)obj);
        }

        public override int GetHashCode()
        {
            return location.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} {1} at {2}".FormatInvariant(faction, prototype, location);
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
