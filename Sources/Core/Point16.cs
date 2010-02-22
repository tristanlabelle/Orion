using System;
using System.Runtime.InteropServices;
using OpenTK.Math;

namespace Orion
{
    /// <summary>
    /// A point structure holding an X and Y coordinate pair as <see cref="Int16"/>s.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size=sizeof(short) * 2)]
    public struct Point16 : IEquatable<Point16>
    {
        #region Instance
        #region Fields
        public readonly short X;
        public readonly short Y;
        #endregion

        #region Constructor
        public Point16(short x, short y)
        {
            this.X = x;
            this.Y = y;
        }
        #endregion

        #region Methods
        public Point ToPoint()
        {
            return new Point(X, Y);
        }
        
        public Vector2 ToVector()
        {
            return new Vector2(X, Y);
        }

        #region Object Model
        /// <summary>
        /// Tests for equality with another <see cref="SmallPoint"/>.
        /// </summary>
        /// <param name="other">A <see cref="SmallPoint"/> to be tested with.</param>
        /// <returns>True this <see name="SmallPoint"/> is equal to <paramref name="other"/>, false if not.</returns>
        public bool Equals(Point16 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point16)) return false;
            return Equals((Point16)obj);
        }

        public override int GetHashCode()
        {
            return ((int)X << 16) | (int)(ushort)Y;
        }

        public override string ToString()
        {
            return "({0}, {1})".FormatInvariant(X, Y);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A point with an X and a Y of zero.
        /// </summary>
        public static readonly Point16 Zero = new Point16();
        #endregion

        #region Methods
        public static Point16 Truncate(Vector2 vector)
        {
            return new Point16((short)vector.X, (short)vector.Y);
        }

        public static Point16 FromPoint(Point point)
        {
            return new Point16((short)point.X, (short)point.Y);
        }

        #region Equality
        /// <summary>
        /// Tests two <see cref="SmallPoint"/> for equality.
        /// </summary>
        /// <param name="first">The first <see cref="SmallPoint"/>.</param>
        /// <param name="second">The second <see cref="SmallPoint"/>.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are equal, false if not.</returns>
        public static bool Equals(Point16 first, Point16 second)
        {
            return first.Equals(second);
        }
        #endregion
        #endregion

        #region Operators
        #region Equality
        /// <summary>
        /// Tests two <see cref="SmallPoint"/> for equality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are equal, false if they are different.
        /// </returns>
        public static bool operator ==(Point16 lhs, Point16 rhs)
        {
            return Equals(lhs, rhs);
        }

        /// <summary>
        /// Tests two <see cref="SmallPoint"/> for inequality.
        /// </summary>
        /// <param name="lhs">The left hand side operand instance.</param>
        /// <param name="rhs">The right hand side operand instance.</param>
        /// <returns>
        /// True if <paramref name="lhs"/> and <paramref name="rhs"/> are different, false if they are equal.
        /// </returns>
        public static bool operator !=(Point16 lhs, Point16 rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion

        #region Casting
        public static implicit operator Vector2(Point16 point)
        {
            return point.ToVector();
        }

        public static implicit operator Point(Point16 point)
        {
            return point.ToPoint();
        }

        public static explicit operator Point16(Vector2 vector)
        {
            return Truncate(vector);
        }

        public static explicit operator Point16(Point point)
        {
            return FromPoint(point);
        }
        #endregion
        #endregion
        #endregion
    }
}
