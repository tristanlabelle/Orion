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
    /// Represents what is remembered of an <see cref="Entity"/> which has been seen but is now in the fog of war.
    /// </summary>
    [ImmutableObject(true)]
    public struct RememberedEntity : IEquatable<RememberedEntity>
    {
        #region Instance
        #region Fields
        private readonly Point position;
        private readonly Entity prototype;
        private readonly Faction faction;
        #endregion

        #region Constructors
        public RememberedEntity(Point position, Entity prototype, Faction faction)
        {
            Argument.EnsureNotNull(prototype, "prototype");

            this.position = position;
            this.prototype = prototype;
            this.faction = faction;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the position of the origin of the remembered entity.
        /// </summary>
        public Point Position
        {
            get { return position; }
        }

        /// <summary>
        /// Gets the grid region occupied by the remembered entity.
        /// </summary>
        public Region GridRegion
        {
            get { return Spatial.GetGridRegion(position, prototype.Spatial.Size); }
        }

        /// <summary>
        /// Gets the prototype of the remembered entity.
        /// </summary>
        public Entity Prototype
        {
            get { return prototype; }
        }

        /// <summary>
        /// Gets the faction of the remembered entity.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if an <see cref="Entity"/> matches this description.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if it matches this description.</returns>
        public bool Matches(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            Entity prototype = Identity.GetPrototype(entity);
            return prototype != null
                && entity.Spatial != null 
                && new RememberedEntity(entity.Spatial.GridRegion.Min, prototype, FactionMembership.GetFaction(entity)) == this;
        }

        public bool Equals(RememberedEntity other)
        {
            return position == other.position
                && prototype == other.prototype
                && faction == other.faction;
        }

        public override bool Equals(object obj)
        {
            return obj is RememberedEntity && Equals((RememberedEntity)obj);
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} {1} at {2}".FormatInvariant(faction, prototype, position);
        }
        #endregion
        #endregion

        #region Static
        public static bool Equals(RememberedEntity a, RememberedEntity b)
        {
            return a.Equals(b);
        }

        public static bool operator ==(RememberedEntity a, RememberedEntity b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(RememberedEntity a, RememberedEntity b)
        {
            return !Equals(a, b);
        }
        #endregion
    }
}
