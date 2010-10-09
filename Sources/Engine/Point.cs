using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using OpenTK;

namespace Orion.Engine
{
    /// <summary>
    /// Represents an (x,y) 2D integral coordinate pair.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(int) * 2)]
    public struct Point : IEquatable<Point>, IFormattable
    {
        #region Instance
        #region Fields
        public readonly int X;
        public readonly int Y;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Size"/> from a width and a height.
        /// </summary>
        /// <param name="x">The x of the size.</param>
        /// <param name="y">The y of the size.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        #endregion

        #region Methods
        public Vector2 ToVector()
        {
            return new Vector2(X, Y);
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Size)) return false;
            return Equals((Size)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "({0},{1})", X, Y);
        }
        #endregion

        #region Explicit Members
        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (format != null) throw new NotSupportedException("Format strings are not supported by Point objects.");
            return ToString(formatProvider);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A point with the x and y coordinates set to zero.
        /// </summary>
        public static readonly Point Zero = new Point(0, 0);
        #endregion

        #region Methods
        public static Point Truncate(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static bool Equals(Point a, Point b)
        {
            return a.Equals(b);
        }
        #endregion

        #region Operators
        public static bool operator ==(Point a, Point b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !Equals(a, b);
        }

        public static explicit operator Point(Vector2 vector)
        {
            return Truncate(vector);
        }

        public static implicit operator Vector2(Point point)
        {
            return point.ToVector();
        }
        #endregion
        #endregion
    }
}
