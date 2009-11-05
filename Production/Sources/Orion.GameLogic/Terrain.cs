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
        /// <param name="width">The width of the terrain to be generated.</param>
        /// <param name="height">The height of the terrain to be generated.</param>
        public Terrain(int width, int height)
        {
            Argument.EnsureStrictlyPositive(width, "width");
            Argument.EnsureStrictlyPositive(height, "height");
            this.tiles = new BitArray2D(width, height);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of this terrain, in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.ColumnCount; }
        }

        /// <summary>
        /// Gets the height of this terrain, in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.RowCount; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Indicates if the tile at a specified coordinate is walkable.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the terrain field.</returns>
        public bool IsWalkable(int x, int y)
        {
            return !tiles[x, y];
        }

        /// <summary>
        /// Indicates if the tile at a specified coordinate is walkable.
        /// </summary>
        /// <param name="position">A <see cref="Vector2"/> holding the coordinates</param>
        /// <returns>A boolean value from the terrain field.</returns>
        public bool IsWalkable(Vector2 position)
        {
            return IsWalkable((int)position.X, (int)position.Y);
        }

        public bool IsWalkable(Point16 point)
        {
            return IsWalkable(point.X, point.Y);
        }

        public bool IsWalkableAndWithinBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;
            return IsWalkable(x, y);
        }

        public bool IsWalkable(LineSegment lineSegment, int width)
        {
            Argument.EnsureStrictlyPositive(width, "width");

            // Bresenham's line algorithm
            // Source: http://en.wikipedia.org/wiki/Bresenham's_line_algorithm

            Vector2 normal = lineSegment.Delta.PerpendicularLeft;
            normal.Normalize();

            for (int i = 0; i < width; ++i)
            {
                Vector2 displacement = normal * (i - width * 0.5f);

                Vector2 p1 = lineSegment.EndPoint1 + displacement;
                Vector2 p2 = lineSegment.EndPoint2 + displacement;

                int x0 = (int)lineSegment.EndPoint1.X;
                int x1 = (int)lineSegment.EndPoint2.X;
                int y0 = (int)lineSegment.EndPoint1.Y;
                int y1 = (int)lineSegment.EndPoint2.Y;

                if (!IsLineWalkable(x0, x1, y0, y1))
                    return false;
            }

            return true;
        }

        private bool IsLineWalkable(int x0, int x1, int y0, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltaX = x1 - x0;
            int deltaY = Math.Abs(y1 - y0);
            int error = deltaX / 2;
            int yStep = (y0 < y1) ? 1 : -1;
            int y = y0;

            for (int x = x0; x < x1; ++x)
            {
                bool isWalkable = steep ? IsWalkableAndWithinBounds(y, x) : IsWalkableAndWithinBounds(x, y);
                if (!isWalkable) return false;

                error = error - deltaY;
                if (error < 0)
                {
                    y += yStep;
                    error += deltaX;
                }
            }

            return true;
        }

        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Returns a <see cref="Terrain"/> generated by a <see cref="PerlinNoise"/>. 
        /// </summary>
        /// <param name="terrainWidth">The width of the terrain to be generated.</param>
        /// <param name="terrainHeight">The height of the terrain to be generated.</param>
        /// <param name="random">The <see cref="Random"/> to be used to generate the terrain.</param>
        /// <returns>A newly generated <see cref="Terrain"/>.</returns>
        public static Terrain Generate(int terrainWidth, int terrainHeight, Random random)
        {
            PerlinNoise noise = new PerlinNoise(random);

            Terrain terrain = new Terrain(terrainWidth, terrainHeight);
            double[] rawTerrain = new double[terrainWidth * terrainHeight];
            for (int i = 0; i < terrainHeight; i++)
            {
                for (int j = 0; j < terrainWidth; j++)
                {
                    rawTerrain[i * terrainWidth + j] = noise[j, i];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                terrain.tiles[k % terrainHeight, k / terrainHeight] = noiseValue >= 0.5;
                k++;
            }

            return terrain;
        }
        #endregion
        #endregion
    }
}
