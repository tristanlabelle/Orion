using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using OpenTK;
using Orion.Engine.Geometry;

namespace Orion.Engine
{
    /// <summary>
    /// Represents a 2D rectangular region.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4 * sizeof(int))]
    [ImmutableObject(true)]
    public struct Region : IEquatable<Region>, IFormattable
    {
        #region Instance
        #region Fields
        private readonly Point min;
        private readonly Size size;
        #endregion

        #region Constructors
        public Region(Point min, Size size)
        {
            this.min = min;
            this.size = size;
        }

        public Region(int minX, int minY, int width, int height)
            : this(new Point(minX, minY), new Size(width, height)) { }

        public Region(Size size)
        {
            this.min = Point.Zero;
            this.size = size;
        }

        public Region(int width, int height)
            : this(new Size(width, height)) { }
        #endregion

        #region Properties
        #region Min
        public Point Min
        {
            get { return min; }
        }

        public int MinX
        {
            get { return min.X; }
        }

        public int MinY
        {
            get { return min.Y; }
        }
        #endregion

        #region Size
        public Size Size
        {
            get { return size; }
        }

        public int Width
        {
            get { return size.Width; }
        }

        public int Height
        {
            get { return size.Height; }
        }
        #endregion

        #region InclusiveMax
        public int InclusiveMaxX
        {
            get
            {
                if (Width == 0)
                {
                    throw new InvalidOperationException(
                        "A zero-width region does not have an inclusive maximum x coordinate.");
                }

                return MinX + Width - 1;
            }
        }

        public int InclusiveMaxY
        {
            get
            {
                if (Height == 0)
                {
                    throw new InvalidOperationException(
                        "A zero-height region does not have an inclusive maximum y coordinate.");
                }

                return MinY + Height - 1;
            }
        }

        public Point InclusiveMax
        {
            get
            {
                if (Size.Area == 0)
                {
                    throw new InvalidOperationException(
                        "A zero-sized region does not have an inclusive maximum coordinate.");
                }

                return new Point(ExclusiveMaxX, ExclusiveMaxY);
            }
        }
        #endregion

        #region ExclusiveMax
        public int ExclusiveMaxX
        {
            get { return MinX + Width; }
        }

        public int ExclusiveMaxY
        {
            get { return MinY + Height; }
        }

        public Point ExclusiveMax
        {
            get { return new Point(ExclusiveMaxX, ExclusiveMaxY); }
        }
        #endregion

        public int Area
        {
            get { return size.Area; }
        }

        public int Perimeter
        {
            get { return size.Width * 2 + size.Height * 2; }
        }

        /// <summary>
        /// Enumerates the points in this region.
        /// </summary>
        public IEnumerable<Point> Points
        {
            get
            {
                int exclusiveMaxX = ExclusiveMaxX;
                int exclusiveMaxY = ExclusiveMaxY;
                for (int x = MinX; x < exclusiveMaxX; ++x)
                    for (int y = MinY; y < exclusiveMaxY; ++y)
                        yield return new Point(x, y);
            }
        }

        /// <summary>
        /// Enumerates the points in the internal border of this region.
        /// </summary>
        public IEnumerable<Point> InternalBorderPoints
        {
            get
            {
                if (Size.Area == 0) yield break;

                Point inclusiveMax = InclusiveMax;

                for (int y = MinY; y < inclusiveMax.Y; ++y)
                    yield return new Point(MinX, y);

                for (int x = MinX; x < inclusiveMax.X; ++x)
                    yield return new Point(x, inclusiveMax.Y);

                for (int y = inclusiveMax.Y; y > MinY; --y)
                    yield return new Point(inclusiveMax.X, y);

                for (int x = inclusiveMax.X; x > MinX; --x)
                    yield return new Point(x, MinY);
            }
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets a point of this region from its relative coordinates.
        /// </summary>
        /// <param name="x">The relative x coordinate of the region's point.</param>
        /// <param name="x">The relative y coordinate of the region's point.</param>
        /// <returns>The point at that location in this region.</returns>
        public Point this[int x, int y]
        {
            get { return new Point(min.X + x, min.Y + y); }
        }

        public Point this[Point point]
        {
            get { return this[point.X, point.Y]; }
        }
        #endregion

        #region Methods
        public Rectangle ToRectangle()
        {
            return new Rectangle(MinX, MinY, Width, Height);
        }

        /// <summary>
        /// Tests if a point is within this region.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns>A value indicating if the point is within this region.</returns>
        public bool Contains(Point point)
        {
            return point.X >= MinX && point.Y >= MinY
                && point.X < ExclusiveMaxX && point.Y < ExclusiveMaxY;
        }

        public bool Contains(Region other)
        {
            return other.MinX >= MinX && other.MinY >= MinY
                && other.ExclusiveMaxX <= ExclusiveMaxX
                && other.ExclusiveMaxY <= ExclusiveMaxY;
        }

        /// <summary>
        /// Clamps a point so its coordinates are within this region.
        /// </summary>
        /// <param name="point">The point to be clamped.</param>
        /// <returns>The resulting clamped point.</returns>
        public Point Clamp(Point point)
        {
            int clampedX = point.X;
            int clampedY = point.Y;
            if (point.X < MinX) clampedX = MinX;
            else if (point.X >= ExclusiveMaxX) clampedX = ExclusiveMaxX - 1;
            if (point.Y < MinY) clampedY = MinY;
            else if (point.Y >= ExclusiveMaxY) clampedY = ExclusiveMaxY - 1;
            return new Point(clampedX, clampedY);
        }

        public Point[] GetAdjacentPoints()
        {
            Point[] points = new Point[Perimeter + 4];
            for (int i = 0; i < Height + 1; ++i)
                points[i] = new Point(MinX - 1, MinY - 1 + i);
            for (int i = 0; i < Width + 1; ++i)
                points[Height + 1 + i] = new Point(MinX - 1 + i, ExclusiveMaxY);
            for (int i = 0; i < Height + 1; ++i)
                points[Height + Width + 2 + i] = new Point(ExclusiveMaxX, ExclusiveMaxY - i);
            for (int i = 0; i < Width + 1; ++i)
                points[Height + Width + Height + 3 + i] = new Point(ExclusiveMaxX - i, MinY - 1);
            return points;
        }

        public bool IsAdjacent(Point point)
        {
            return (point.X == MinX - 1 || point.X == ExclusiveMaxX)
                && (point.Y == MinY - 1 || point.Y == ExclusiveMaxY);
        }

        /// <summary>
        /// Gets the coordinates of a point in this rectangle's coordinate system,
        /// where (0,0) is the minimum corner and (1,1) is the maximum corner.
        /// </summary>
        /// <param name="point">The point to be normalized.</param>
        /// <returns>The resulting normalized point.</returns>
        public Vector2 Normalize(Point point)
        {
            return new Vector2((point.X - MinX) / (float)Width, (point.Y - MinY) / (float)Height);
        }

        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(Region other)
        {
            return min == other.min && size == other.size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Region)) return false;
            return Equals((Region)obj);
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ size.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "{0} {1}", min, size);
        }
        #endregion

        #region Explicit Members
        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (format != null) throw new NotSupportedException();
            return ToString(formatProvider);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        #region Factory Methods
        public static Region FromMinInclusiveMax(Point min, Point inclusiveMax)
        {
            Size size = new Size(inclusiveMax.X - min.X + 1, inclusiveMax.Y - min.Y + 1);
            return new Region(min, size);
        }

        public static Region FromMinInclusiveMax(int minX, int minY, int inclusiveMaxX, int inclusiveMaxY)
        {
            Point min = new Point(minX, minY);
            Point inclusiveMax = new Point(inclusiveMaxX, inclusiveMaxY);
            return FromMinInclusiveMax(min, inclusiveMax);
        }

        public static Region FromMinExclusiveMax(Point min, Point exclusiveMax)
        {
            Size size = new Size(exclusiveMax.X - min.X, exclusiveMax.Y - min.Y);
            return new Region(min, size);
        }

        public static Region FromMinExclusiveMax(int minX, int minY, int exclusiveMaxX, int exclusiveMaxY)
        {
            return FromMinExclusiveMax(new Point(minX, minY), new Point(exclusiveMaxX, exclusiveMaxY));
        }

        #region FromPoints
        public static Region FromPoints(Point first, Point second)
        {
            int minX, maxX;
            GetMinMax(first.X, second.X, out minX, out maxX);

            int minY, maxY;
            GetMinMax(first.Y, second.Y, out minY, out maxY);

            return FromMinInclusiveMax(minX, minY, maxX, maxY);
        }

        public static Region FromPoints(int firstX, int firstY, int secondX, int secondY)
        {
            Point first = new Point(firstX, firstY);
            Point second = new Point(secondX, secondY);
            return FromPoints(first, second);
        }

        private static void GetMinMax(int a, int b, out int min, out int max)
        {
            if (a < b)
            {
                min = a;
                max = b;
            }
            else
            {
                min = b;
                max = a;
            }
        }
        #endregion
        #endregion

        public static Region Grow(Region region, int amount)
        {
            Argument.EnsurePositive(amount, "amount");
            return new Region(
                region.MinX - amount, region.MinY - amount,
                region.Width + amount * 2, region.Height + amount * 2);
        }

        #region Boolean Operations
        public static Region Union(Region a, Region b)
        {
            Point min = new Point(Math.Min(a.min.X, b.min.X), Math.Min(a.min.Y, b.min.Y));
            Point exclusiveMax = new Point(
                Math.Max(a.ExclusiveMax.X, b.ExclusiveMax.X),
                Math.Max(a.ExclusiveMax.Y, b.ExclusiveMax.Y));
            return FromMinExclusiveMax(min, exclusiveMax);
        }

        public static Region? Intersection(Region a, Region b)
        {
            int minX = Math.Max(a.MinX, b.MinX);
            int minY = Math.Max(a.MinY, b.MinY);
            int exclusiveMaxX = Math.Min(a.ExclusiveMaxX, b.ExclusiveMaxX);
            int exclusiveMaxY = Math.Min(a.ExclusiveMaxY, b.ExclusiveMaxY);

            if (minX >= exclusiveMaxX || minY >= exclusiveMaxY) return null;
            return Region.FromMinExclusiveMax(minX, minY, exclusiveMaxX, exclusiveMaxY);
        }

        public static bool Intersects(Region a, Region b)
        {
            return Intersection(a, b).HasValue;
        }

        public static bool AreAdjacentOrIntersecting(Region a, Region b)
        {
            return Intersects(Region.Grow(a, 1), b);
        }
        #endregion

        #region Distance
        public static float SquaredDistance(Region a, Region b)
        {
            Point clamped1 = a.Clamp(b.Min);
            Point clamped2 = b.Clamp(clamped1);
            return ((Vector2)clamped1 - (Vector2)clamped2).LengthSquared;
        }

        public static float Distance(Region a, Region b)
        {
            float squaredDistance = SquaredDistance(a, b);
            return (float)Math.Sqrt(squaredDistance);
        }
        #endregion

        public static bool Equals(Region a, Region b)
        {
            return a.Equals(b);
        }
        #endregion

        #region Operators
        public static bool operator ==(Region a, Region b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Region a, Region b)
        {
            return !Equals(a, b);
        }

        public static explicit operator Region(Size size)
        {
            return new Region(size);
        }

        public static implicit operator Rectangle(Region region)
        {
            return new Rectangle(region.MinX, region.MinY, region.Width, region.Height);
        }
        #endregion
        #endregion
    }
}
