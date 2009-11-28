using System;
using System.Linq;

using OpenTK.Math;
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
        private readonly BitArray2D tiles;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Terrain"/> object. 
        /// </summary>
        /// <param name="size">The size of the terrain to be generated.</param>
        public Terrain(Size size)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");
            this.tiles = new BitArray2D(size.Width, size.Height);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of this terrain, in tiles.
        /// </summary>
        public Size Size
        {
            get { return new Size(tiles.ColumnCount, tiles.RowCount); }
        }
        #endregion

        #region Methods
        public bool IsWalkable(Point point)
        {
            return !tiles[point.X, point.Y];
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

        private bool IsWalkable(Region region)
        {
            for (int x = region.MinX; x < region.ExclusiveMaxX; ++x)
                for (int y = region.MinY; y < region.ExclusiveMaxY; ++y)
                    if (!IsWalkable(new Point(x, y)))
                        return false;
            return true;
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

            Terrain terrain = new Terrain(size);
            double[] rawTerrain = new double[size.Width * size.Height];
            for (int i = 0; i < size.Height; i++)
            {
                for (int j = 0; j < size.Width; j++)
                {
                    rawTerrain[i * size.Width + j] = noise[j, i];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                terrain.tiles[k % size.Height, k / size.Height] = noiseValue >= 0.5;
                k++;
            }

            return terrain;
        }
        #endregion
        #endregion
    }
}
