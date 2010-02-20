using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// Represents a ruin which is drawn where a building was destroyed or a unit killed.
    /// </summary>
    internal sealed class Ruin
    {
        #region Fields
        private float creationTimeInSeconds;
        private RuinType type;
        private Vector2 min;
        private Size size;
        private Faction faction;
        #endregion

        #region Properties
        public float CreationTimeInSeconds
        {
            get { return creationTimeInSeconds; }
        }

        public RuinType Type
        {
            get { return type; }
        }

        public Vector2 Min
        {
            get { return min; }
        }

        public Size Size
        {
            get { return size; }
        }

        public Faction Faction
        {
            get { return faction; }
        }

        public ColorRgb Tint
        {
            get { return faction.Color; }
        }
        #endregion

        #region Methods
        public void Reset(float creationTimeInSeconds, RuinType type,
            Vector2 min, Size size, Faction faction)
        {
            this.creationTimeInSeconds = creationTimeInSeconds;
            this.type = type;
            this.min = min;
            this.size = size;
            this.faction = faction;
        }
        #endregion
    }

    internal enum RuinType
    {
        Building,
        Unit
    }
}
