using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Matchmaking.TowerDefense
{
    /// <summary>
    /// Represents the path which is followed by the creeps.
    /// </summary>
    public sealed class CreepPath
    {
        #region Instance
        #region Fields
        private readonly Size terrainSize;
        private readonly ReadOnlyCollection<Point> points;
        private readonly int width;
        #endregion

        #region Constructor
        public CreepPath(Size terrainSize, IEnumerable<Point> points, int width)
        {
            this.terrainSize = terrainSize;
            this.points = points.ToList().AsReadOnly();
            this.width = width;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of the terrain on which this path lies.
        /// </summary>
        public Size TerrainSize
        {
            get { return terrainSize; }
        }

        /// <summary>
        /// Gets the points on the path followed by the creeps.
        /// </summary>
        public ReadOnlyCollection<Point> Points
        {
            get { return points; }
        }

        /// <summary>
        /// Gets the width of the path followed by the creeps, in tiles.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        private IEnumerable<Region> PathSegmentRegions
        {
            get
            {
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    Point firstPathPoint = points[i];
                    Point secondPathPoint = points[i + 1];
                    
                    yield return new Region(
                        Math.Min(firstPathPoint.X, secondPathPoint.X) - width / 2,
                        Math.Min(firstPathPoint.Y, secondPathPoint.Y) - width / 2,
                        Math.Abs(secondPathPoint.X - firstPathPoint.X) + width,
                        Math.Abs(secondPathPoint.Y - firstPathPoint.Y) + width);
                }
            }
        }
        #endregion

        #region Methods
        public bool Contains(Point point)
        {
            return PathSegmentRegions.Any(region => region.Contains(point));
        }

        public BitArray2D GenerateBitmap()
        {
            Region terrainRegion = new Region(terrainSize);

            BitArray2D bitmap = new BitArray2D(terrainSize, false);
            PathSegmentRegions.SelectMany(region => region.Points)
                .Where(point => terrainRegion.Contains(point))
                .ForEach(point => bitmap[point] = true);

            return bitmap;
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Generates a new random path.
        /// </summary>
        /// <param name="terrainSize">The size of the terrain for which to generate a path.</param>
        /// <param name="random">The random-number generator to be used.</param>
        /// <returns>A newly created path to be followed by creeps.</returns>
        public static CreepPath Generate(Size terrainSize, Random random)
        {
            Argument.EnsureNotNull(random, "random");

            List<Point> points = new List<Point>();
            points.Add(new Point(0, terrainSize.Height / 2));

            while (true)
            {
                Point previousPoint = points[points.Count - 1];

                int newX = previousPoint.X + 5 + random.Next(15);
                if (newX >= terrainSize.Width)
                {
                    points.Add(new Point(terrainSize.Width - 1, previousPoint.Y));
                    break;
                }

                points.Add(new Point(newX, previousPoint.Y));
                int newY = 2 + random.Next(terrainSize.Height - 4);
                if (Math.Abs(newY - previousPoint.Y) > 3) points.Add(new Point(newX, newY));
            }

            return new CreepPath(terrainSize, points, 3);
        }
        #endregion
        #endregion
    }
}
