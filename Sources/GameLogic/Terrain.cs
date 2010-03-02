using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Math;
using Orion.Collections;
using Orion.Geometry;

namespace Orion.GameLogic
{
    /// <summary>
    /// Defines a terrain to be drawn in background, parts of this terrain are walkable, others are not.
    /// </summary>
    public sealed class Terrain
    {
        #region Instance
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
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Returns a <see cref="Terrain"/> generated by a <see cref="PerlinNoise"/>. 
        /// </summary>
        /// <param name="size">The size of the terrain to be generated.</param>
        /// <param name="random">The <see cref="Random"/> to be used to generate the terrain.</param>
        /// <returns>A newly generated <see cref="Terrain"/>.</returns>
        public static Terrain Generate(Size size, Random random)
        {
            PerlinNoise noise = new PerlinNoise(random);

            BitArray2D tiles = new BitArray2D(size);
            double[] rawTerrain = new double[size.Area];
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    rawTerrain[y * size.Width + x] = noise[x, y];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                tiles[k % size.Width, k / size.Width] = noiseValue >= 0.5;
                k++;
            }

            return new Terrain(tiles);
        }
        #endregion
        #endregion
    }
}