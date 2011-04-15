using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Specifies the shape of the fog of war region represented by a <see cref="FogOfWarToken"/>.
    /// </summary>
    public enum FogOfWarRegionType
    {
        /// <summary>
        /// Specifies that the fog of war region is a line-of-sight circle.
        /// </summary>
        LineOfSight,

        /// <summary>
        /// Specifies that the fog of war region is a glow
        /// around the entity (used for in-construction buildings).
        /// </summary>
        Glow,

        /// <summary>
        /// Specifies no region in the fog of war.
        /// </summary>
        None
    }

    /// <summary>
    /// Reserves an entity's place in a <see cref="FogOfWar"/>.
    /// </summary>
    public sealed class FogOfWarToken : IDisposable
    {
        #region Fields
        private readonly FogOfWar fogOfWar;
        private FogOfWarRegionType regionType;
        private Vector2 center;
        private readonly Size size;
        private float sightRange;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="FogOfWarToken"/> representing an entity's
        /// trace in a given <see cref="FogOfWar"/>.
        /// </summary>
        /// <param name="fogOfWar">The affected <see cref="FogOfWar"/>.</param>
        /// <param name="regionType">The type of fog of war region used by the entity.</param>
        /// <param name="center">The center of the entity.</param>
        /// <param name="size">The entity's size.</param>
        /// <param name="sightRange">The sight range of the entity.</param>
        /// <returns>A new <see cref="FogOfWarToken"/>.</returns>
        public FogOfWarToken(FogOfWar fogOfWar, FogOfWarRegionType regionType,
            Vector2 center, Size size, float sightRange)
        {
            Argument.EnsureNotNull(fogOfWar, "fogOfWar");
            Argument.EnsurePositive(sightRange, "sightRange");

            this.fogOfWar = fogOfWar;
            this.regionType = regionType;
            this.center = center;
            this.size = size;
            this.sightRange = sightRange;

            if (regionType != FogOfWarRegionType.None)
                AddRegion();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="FogOfWar"/> affected by this token.
        /// </summary>
        public FogOfWar FogOfWar
        {
            get { return fogOfWar; }
        }

        /// <summary>
        /// Accesses the type of the fog of war region affected by this token.
        /// </summary>
        public FogOfWarRegionType RegionType
        {
            get { return regionType; }
            set
            {
                if (value == regionType) return;

                RemoveRegion();
                regionType = value;
                AddRegion();
            }
        }

        /// <summary>
        /// Accesses the location of the center of the entity using this token.
        /// </summary>
        public Vector2 Center
        {
            get { return center; }
            set
            {
                if (value == center) return;

                if (regionType == FogOfWarRegionType.None)
                {
                    center = value;
                }
                else
                {
                    // TODO: Do not update the fog of war for insignificant position changes
                    RemoveRegion();
                    center = value;
                    AddRegion();
                }
            }
        }

        /// <summary>
        /// Gets the size of the entity using this token.
        /// </summary>
        public Size Size
        {
            get { return size; }
        }

        /// <summary>
        /// Accesses the sight range of the entity using this token.
        /// </summary>
        public float SightRange
        {
            get { return sightRange; }
            set
            {
                Argument.EnsurePositive(value, "SightRange");
                if (value == sightRange) return;

                if (regionType == FogOfWarRegionType.LineOfSight)
                {
                    RemoveRegion();
                    sightRange = value;
                    AddRegion();
                }
                else
                {
                    sightRange = value;
                }
            }
        }

        /// <summary>
        /// Gets the line-of-sight circle represented by this token.
        /// </summary>
        public Circle LineOfSight
        {
            get { return new Circle(center, sightRange); }
        }

        /// <summary>
        /// Gets the rectangular entity region represented by this token.
        /// </summary>
        public Region Region
        {
            get { return Spatial.GetGridRegion(center - (Vector2)size * 0.5f, size); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Removes any fog of war usage by this token.
        /// </summary>
        public void Dispose()
        {
            RegionType = FogOfWarRegionType.None;
        }

        private void AddRegion()
        {
            if (regionType == FogOfWarRegionType.LineOfSight)
                fogOfWar.AddLineOfSight(LineOfSight);
            else if (regionType == FogOfWarRegionType.Glow)
                fogOfWar.AddRegion(Region);
        }

        private void RemoveRegion()
        {
            if (regionType == FogOfWarRegionType.LineOfSight)
                fogOfWar.RemoveLineOfSight(LineOfSight);
            else if (regionType == FogOfWarRegionType.Glow)
                fogOfWar.RemoveRegion(Region);
        }
        #endregion
    }
}
