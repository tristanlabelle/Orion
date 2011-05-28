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
        /// Values indicating the solidity of every tile.
        /// The tile values can be casted to <see cref="TileType"/> instances.
        /// </summary>
        private readonly byte[] tiles;
        private readonly int width;
        private readonly int height;
        #endregion

        #region Constructors
        public Terrain(Size size)
        {
            this.width = size.Width;
            this.height = size.Height;
            this.tiles = new byte[size.Area];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of this terrain, in tiles.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height of this terrain, in tiles.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the size of this terrain, in tiles.
        /// </summary>
        public Size Size
        {
            get { return new Size(width, height); }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Accesses the type of a tile at a given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <returns>The type of the tile at those coordinates</returns>
        /// <remarks>
        /// Be very careful setting tiles after the game has been initialized.
        /// </remarks>
        public TileType this[int x, int y]
        {
            get
            {
                Debug.Assert(x >= 0 && y >= 0 && x < width && y < height);
                return (TileType)tiles[x + y * width];
            }
            set
            {
                Debug.Assert(x >= 0 && y >= 0 && x < width && y < height);
                tiles[x + y * width] = (byte)value;
            }
        }

        /// <summary>
        /// Accesses the type of a tile at a given coordinate.
        /// </summary>
        /// <param name="point">The coordinate of the tile.</param>
        /// <returns>The type of the tile at those coordinates</returns>
        /// <remarks>
        /// Be very careful setting tiles after the game has been initialized.
        /// </remarks>
        public TileType this[Point point]
        {
            get { return this[point.X, point.Y]; }
            set { this[point.X, point.Y] = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Obtains the type of a tile at a given coordinate, considering
        /// out-of-bounds tiles as obstacles.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <returns>
        /// The type of the tile at those coordinates,
        /// or <see cref="TileType.Obstacle"/> if the coordinates were out of bounds.
        /// </returns>
        public TileType GetTileTypeOrObstacle(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height
                ? (TileType)tiles[x + y * width] : TileType.Obstacle;
        }

        /// <summary>
        /// Tests if a given terrain region is fully walkable.
        /// </summary>
        /// <param name="region">The terrain region to be tested.</param>
        /// <returns>A value indicating if it is fully walkable.</returns>
        public bool IsWalkable(Region region)
        {
            int exclusiveMaxX = region.ExclusiveMaxX;
            int exclusiveMaxY = region.ExclusiveMaxY;
            Debug.Assert(region.MinX >= 0 && region.MinY >= 0
                && exclusiveMaxX <= width && exclusiveMaxY <= height);

            for (int y = region.MinY; y < exclusiveMaxY; ++y)
                for (int x = region.MinX; x < exclusiveMaxX; ++x)
                    if ((TileType)tiles[x + y * width] != TileType.Walkable)
                        return false;

            return true;
        }
        #endregion
    }
}
