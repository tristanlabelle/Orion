using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components.Serialization;
using OpenTK;
using System.Diagnostics;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to have a line-of-sight radius.
    /// </summary>
    public sealed class Vision : Component
    {
        #region Fields
        public static readonly Stat RangeStat = new Stat(typeof(Vision), StatType.Integer, "Range");

        private int range = 1;
        private FogOfWarToken fogOfWarToken;
        #endregion

        #region Constructors
        public Vision(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the radius of the line-of-sight circle around this <see cref="Entity"/>.
        /// </summary>
        [Persistent(true)]
        public int Range
        {
            get { return range; }
            set
            {
                Argument.EnsurePositive(value, "Range");
                range = value;
            }
        }

        /// <summary>
        /// Gets the circle containing the part of the world seen by this <see cref="Entity"/>.
        /// </summary>
        public Circle LineOfSight
        {
            get
            {
                Spatial spatial = Entity.Spatial;
                Vector2 center = spatial == null ? new Vector2(float.NaN, float.NaN) : spatial.Center;
                return new Circle(center, (float)Entity.GetStatValue(RangeStat));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a value indicating if this <see cref="Entity"/> can see a given point.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns><c>True</c> if the <see cref="point"/> can be seen, <c>false</c> if not.</returns>
        public bool IsInRange(Vector2 point)
        {
            Spatial spatial = Entity.Spatial;
            if (spatial == null) return false;

            Circle lineOfSight = new Circle(spatial.Center, (float)Entity.GetStatValue(RangeStat));
            return lineOfSight.ContainsPoint(point);
        }

        /// <summary>
        /// Tests if an <see cref="Entity"/> is within the line of sight of this <see cref="Entity"/>.
        /// </summary>
        /// <param name="other">The <see cref="Entity"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if it is within the line of sight of this <see cref="Entity"/>, <c>false</c> if not.
        /// </returns>
        public bool IsInRange(Entity other)
        {
            Argument.EnsureNotNull(other, "other");

            Spatial spatial = Entity.Spatial;
            return spatial != null && spatial.IsInRange(other, range);
        }

        protected override void Update(SimulationStep step)
        {
            Spatial spatial = Entity.Spatial;
            Faction faction = FactionMembership.GetFaction(Entity);
            FogOfWarRegionType regionType = Entity.Components.Has<BuildProgress>()
                ? FogOfWarRegionType.Glow : FogOfWarRegionType.LineOfSight;

            if (fogOfWarToken != null)
            {
                if (spatial == null || faction == null || fogOfWarToken.FogOfWar != faction.LocalFogOfWar)
                {
                    fogOfWarToken.Dispose();
                    fogOfWarToken = null;
                }
                else
                {
                    // NOPs if unchanged
                    fogOfWarToken.Center = spatial.Center;
                    fogOfWarToken.SightRange = (float)Entity.GetStatValue(RangeStat);
                    fogOfWarToken.RegionType = regionType;
                    Debug.Assert(fogOfWarToken.Size == spatial.Size, "The size of the entity changed unexpectedly.");
                }
            }

            if (fogOfWarToken == null && spatial != null && faction != null)
            {
                fogOfWarToken = new FogOfWarToken(faction.LocalFogOfWar, regionType,
                    spatial.Center, spatial.Size, (float)Entity.GetStatValue(RangeStat));
            }
        }

        protected override void Deactivate()
        {
            if (fogOfWarToken != null)
            {
                fogOfWarToken.Dispose();
                fogOfWarToken = null;
            }
        }
        #endregion
    }
}
