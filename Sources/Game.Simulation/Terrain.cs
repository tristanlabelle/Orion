using System;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Defines a terrain to be drawn in background, parts of this terrain are walkable, others are not.
    /// </summary>
    public sealed class Terrain
    {
        #region Fields
        /// <summary>
        /// Values indicating the walkability of every tile.
        /// </summary>
        /// <remarks>
        /// Stored as a bool array as it offers the best retrieval time.
        /// </remarks>
        private readonly bool[] tiles;
        private readonly Size size;
        #endregion

        #region Constructors
        public Terrain(BitArray2D tiles)
        {
            Argument.EnsureNotNull(tiles, "tiles");
            this.tiles = new bool[tiles.Area];
            tiles.Bits.CopyTo(this.tiles, 0);
            this.size = tiles.Size;
        }
        #endregion

        #region Properties
        public int Width
        {
            get { return size.Width; }
        }

        public int Height
        {
            get { return size.Height; }
        }

        /// <summary>
        /// Gets the size of this terrain, in tiles.
        /// </summary>
        public Size Size
        {
            get { return size; }
        }
        #endregion

        #region Methods
        public bool IsWalkable(Point point)
        {
            Debug.Assert(point.X >= 0 && point.Y >= 0 && point.X < size.Width && point.Y < size.Height);
            int index = point.Y * size.Width + point.X;
            return !tiles[index];
        }

        public bool IsWalkable(Rectangle rectangle)
        {
            int minX = Math.Max(0, (int)rectangle.MinX);
            int minY = Math.Max(0, (int)rectangle.MinY);
            int maxX = Math.Min(Size.Width - 1, (int)rectangle.MaxX);
            int maxY = Math.Min(Size.Height - 1, (int)rectangle.MaxY);

            Region region = Region.FromMinInclusiveMax(new Point(minX, minY), new Point(maxX, maxY));
            return IsWalkable(region);
        }

        public bool IsWalkable(Region region)
        {
            return region.Points.All(point => IsWalkable(point));
        }

        public bool IsWalkableAndWithinBounds(Point point)
        {
            Region region = (Region)Size;
            if (!region.Contains(point)) return false;
            return IsWalkable(point);
        }
        #endregion
    }
}
