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
        public Point Min
        {
            get { return min; }
        }

        public Size Size
        {
            get { return size; }
        }

        public Point ExclusiveMax
        {
            get { return new Point(min.X + size.Width, min.Y + size.Height); }
        }
        #endregion

        #region Methods
        public bool Contains(Point point)
        {
            return point.X >= min.X && point.Y >= min.Y
                && point.X < ExclusiveMax.X && point.Y < ExclusiveMax.Y;
        }

        public Point Clamp(Point point)
        {
            int clampedX = point.X;
            int clampedY = point.Y;
            if (point.X < min.X) clampedX = min.X;
            else if (point.X >= ExclusiveMax.X) clampedX = ExclusiveMax.X - 1;
            if (point.Y < min.Y) clampedY = min.Y;
            else if (point.Y >= ExclusiveMax.Y) clampedY = ExclusiveMax.Y - 1;
            return new Point(clampedX, clampedY);
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
