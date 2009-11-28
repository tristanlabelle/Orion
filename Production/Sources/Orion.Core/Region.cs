using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Orion
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
            Argument.EnsurePositive(min.X, "min.X");
            Argument.EnsurePositive(min.Y, "min.Y");

            this.min = min;
            this.size = size;
        }

        public Region(Size size)
        {
            this.min = Point.Zero;
            this.size = size;
        }
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
            for (int i = 0; i < Height; ++i)
                points[i] = new Point(MinX - 1, MinY - 1 + i);
            for (int i = 0; i < Width; ++i)
                points[Height + i] = new Point(MinX - 1 + i, ExclusiveMaxY);
            for (int i = 0; i < Height; ++i)
                points[Height + Width + i] = new Point(ExclusiveMaxX, ExclusiveMaxY - i);
            for (int i = 0; i < Width; ++i)
                points[Height + Width + Height + i] = new Point(ExclusiveMaxX - i, MinY - 1);
            return points;
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
        public static Region FromMinInclusiveMax(Point min, Point inclusiveMax)
        {
            Size size = new Size(inclusiveMax.X - min.X + 1, inclusiveMax.Y - min.Y + 1);
            return new Region(min, size);
        }

        public static Region FromMinExclusiveMax(Point min, Point exclusiveMax)
        {
            Size size = new Size(exclusiveMax.X - min.X, exclusiveMax.Y - min.Y);
            return new Region(min, size);
        }

        public static Region Union(Region a, Region b)
        {
            Point min = new Point(Math.Min(a.min.X, b.min.X), Math.Min(a.min.Y, b.min.Y));
            Point exclusiveMax = new Point(
                Math.Max(a.ExclusiveMax.X, b.ExclusiveMax.X),
                Math.Max(a.ExclusiveMax.Y, b.ExclusiveMax.Y));
            return FromMinExclusiveMax(min, exclusiveMax);
        }

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
        #endregion
        #endregion
    }
}
