using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components;
using Orion.Engine;
using System.Diagnostics;
using OpenTK;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Holds a grid indicating which tiles are occupied by which <see cref="Spatial"/> component.
    /// </summary>
    internal sealed class SpatialGrid
    {
        #region Fields
        private readonly int width;
        private readonly int height;
        private readonly Spatial[] grid;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="SpatialGrid"/> from its size.
        /// </summary>
        /// <param name="size">The size of the grid, in cells.</param>
        public SpatialGrid(Size size)
        {
            this.width = size.Width;
            this.height = size.Height;
            this.grid = new Spatial[size.Area];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of this grid.
        /// </summary>
        public Size Size
        {
            get { return new Size(width, height); }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the <see cref="Spatial"/> occupying the ground tile at a given coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the tile.</param>
        /// <param name="y">The y coordinate of the tile.</param>
        /// <returns>The <see cref="Spatial"/> occupying that tile, if any.</returns>
        public Spatial this[int x, int y]
        {
            get { return grid[GetGridIndex(x, y)]; }
            internal set { grid[GetGridIndex(x, y)] = value; }
        }

        public Spatial this[Point point]
        {
            get { return grid[GetGridIndex(point.X, point.Y)]; }
            internal set { grid[GetGridIndex(point.X, point.Y)] = value; }
        }
        #endregion

        #region Methods
        public void Add(Spatial spatial, Region region)
        {
            for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
            {
                for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                {
                    int index = GetGridIndex(x, y);
                    Debug.Assert(grid[index] == null, "Cannot add an entity where there is already one.");
                    grid[index] = spatial;
                }
            }
        }

        public void Remove(Spatial spatial, Region region)
        {
            for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
            {
                for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                {
                    int index = GetGridIndex(x, y);
                    Debug.Assert(grid[index] == spatial, "Unexpected entity when removing from spatial grid.");
                    grid[index] = null;
                }
            }
        }

        public void UpdatePosition(Spatial spatial, Vector2 previousPosition, Vector2 newPosition)
        {
            Point previousGridRegionMinPoint = Spatial.GetGridRegionMinPoint(previousPosition);
            Point newGridRegionMinPoint = Spatial.GetGridRegionMinPoint(newPosition);
            if (newGridRegionMinPoint == previousGridRegionMinPoint) return;

            Remove(spatial, new Region(previousGridRegionMinPoint, spatial.Size));
            Add(spatial, new Region(newGridRegionMinPoint, spatial.Size));
        }

        private int GetGridIndex(int x, int y)
        {
            return x + y * width;
        }
        #endregion
    }
}
