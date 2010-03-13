using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;

namespace Orion.GameLogic.Utilities
{
    /// <summary>
    /// Represents a ruin which is drawn where a building was destroyed or a unit killed.
    /// </summary>
    public sealed class Ruin
    {
        #region Fields
        private readonly Faction faction;
        private readonly UnitType unitType;
        private readonly Vector2 center;
        private float remainingTimeToLive;
        #endregion

        #region Constructors
        internal Ruin(Faction faction, UnitType unitType, Vector2 center, float lifeSpan)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(unitType, "unitType");

            this.faction = faction;
            this.unitType = unitType;
            this.center = center;
            this.remainingTimeToLive = lifeSpan;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the color of the faction of the unit which died.
        /// </summary>
        public ColorRgb FactionColor
        {
            get { return faction.Color; }
        }

        /// <summary>
        /// Gets a value indicating if the unit that died was a building or a unit.
        /// </summary>
        public bool WasBuilding
        {
            get { return unitType.IsBuilding; }
        }

        /// <summary>
        /// Gets the bounding rectangle of this ruin.
        /// </summary>
        public Rectangle Rectangle
        {
            get { return Rectangle.FromCenterSize(center.X, center.Y, unitType.Width, unitType.Height); }
        }

        /// <summary>
        /// Gets the amount of time, in seconds, this ruin will exist.
        /// </summary>
        public float RemainingTimeToLive
        {
            get { return remainingTimeToLive; }
        }

        /// <summary>
        /// Gets a value indicating if this ruin has died due to its age.
        /// </summary>
        public bool IsDead
        {
            get { return remainingTimeToLive <= 0; }
        }
        #endregion

        #region Methods
        internal void Update(float timeDelta)
        {
            remainingTimeToLive -= timeDelta;
            if (remainingTimeToLive < 0) remainingTimeToLive = 0;
        }
        #endregion
    }
}
