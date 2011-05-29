using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Orion.Engine
{
    /// <summary>
    /// A 2D fixed-point vector.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct Fixed2 : IEquatable<Fixed2>
    {
        #region Instance
        #region Fields
        public Fixed X;
        public Fixed Y;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new vector from its X and Y components.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        public Fixed2(Fixed x, Fixed y)
        {
            this.X = x;
            this.Y = y;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the squared length of this vector.
        /// </summary>
        public Fixed SquaredLength
        {
            get
            {
                long scaledResult = (long)X.RawValue * (long)X.RawValue
                    + (long)Y.RawValue * (long)Y.RawValue;
                return (int)(scaledResult >> (Fixed.FractionalBitCount * 2));
            }
        }

        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public Fixed Length
        {
            get { return Fixed.Sqrt(SquaredLength); }
        }
        #endregion

        #region Methods
        public bool Equals(Fixed2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixed2 && Equals((Fixed2)obj);
        }

        public override int GetHashCode()
        {
            return ((X.RawValue << 16) ^ (X.RawValue >> 16)) ^ Y.RawValue;
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append('[')
                .Append(X)
                .Append(' ')
                .Append(Y)
                .Append(']')
                .ToString();
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A vector with a value of zero.
        /// </summary>
        public static readonly Fixed2 Zero = new Fixed2(0, 0);

        /// <summary>
        /// A vector with both components set to one.
        /// </summary>
        public static readonly Fixed2 One = new Fixed2(1, 1);

        /// <summary>
        /// A vector with a value of one along the X axis.
        /// </summary>
        public static readonly Fixed2 UnitX = new Fixed2(1, 0);

        /// <summary>
        /// A vector with a value of one along the Y axis.
        /// </summary>
        public static readonly Fixed2 UnitY = new Fixed2(0, 1);
        #endregion

        #region Methods
        public static bool Equals(Fixed2 first, Fixed2 second)
        {
            return first.Equals(second);
        }

        #region Additive
        public static Fixed2 Negate(Fixed2 value)
        {
            return new Fixed2(-value.X, -value.Y);
        }

        public static Fixed2 Add(Fixed2 first, Fixed2 second)
        {
            return new Fixed2(first.X + second.X, first.Y + second.Y);
        }

        public static Fixed2 Subtract(Fixed2 first, Fixed2 second)
        {
            return new Fixed2(first.X - second.X, first.Y - second.Y);
        }
        #endregion

        #region Multiplicative
        public static Fixed2 Multiply(Fixed2 vector, Fixed scalar)
        {
            return new Fixed2(vector.X * scalar, vector.Y * scalar);
        }

        public static Fixed2 Multiply(Fixed2 vector, int scalar)
        {
            return new Fixed2(vector.X * scalar, vector.Y * scalar);
        }

        public static Fixed2 Divide(Fixed2 vector, Fixed scalar)
        {
            return new Fixed2(vector.X / scalar, vector.Y / scalar);
        }

        public static Fixed2 Divide(Fixed2 vector, int scalar)
        {
            return new Fixed2(vector.X / scalar, vector.Y / scalar);
        }
        #endregion

        public static Fixed Dot(Fixed2 first, Fixed2 second)
        {
            return first.X * second.X + first.Y * second.Y;
        }

        #region Normalization
        public static Fixed2 Normalize(Fixed2 vector)
        {
            Fixed length = vector.Length;
            return length == 0 ? Fixed2.Zero : vector / length;
        }

        public static Fixed2 Normalize(Fixed2 vector, out Fixed length)
        {
            length = vector.Length;
            return length == 0 ? Fixed2.Zero : vector / length;
        }
        #endregion

        #region Utility
        public static Fixed2 Min(Fixed2 first, Fixed2 second)
        {
            return new Fixed2(Fixed.Min(first.X, second.X), Fixed.Min(first.Y, second.Y));
        }
        
        public static Fixed2 Max(Fixed2 first, Fixed2 second)
        {
            return new Fixed2(Fixed.Max(first.X, second.X), Fixed.Max(first.Y, second.Y));
        }

        public static Fixed2 Clamp(Fixed2 value, Fixed2 min, Fixed2 max)
        {
            return new Fixed2(Fixed.Clamp(value.X, min.X, max.X), Fixed.Clamp(value.Y, min.Y, max.Y));
        }

        public static Fixed2 Lerp(Fixed2 a, Fixed2 b, Fixed blend)
        {
            return a + (b - a) * blend;
        }
        #endregion
        #endregion

        #region Operators
        #region Additive
        public static Fixed2 operator -(Fixed2 value)
        {
            return Negate(value);
        }

        public static Fixed2 operator +(Fixed2 lhs, Fixed2 rhs)
        {
            return Add(lhs, rhs);
        }

        public static Fixed2 operator -(Fixed2 lhs, Fixed2 rhs)
        {
            return Subtract(lhs, rhs);
        }
        #endregion

        #region Multiplicative
        public static Fixed2 operator *(Fixed2 lhs, Fixed rhs)
        {
            return Multiply(lhs, rhs);
        }

        public static Fixed2 operator *(Fixed2 lhs, int rhs)
        {
            return Multiply(lhs, rhs);
        }

        public static Fixed2 operator *(int lhs, Fixed2 rhs)
        {
            return Multiply(rhs, lhs);
        }

        public static Fixed2 operator /(Fixed2 lhs, Fixed rhs)
        {
            return Divide(lhs, rhs);
        }

        public static Fixed2 operator /(Fixed2 lhs, int rhs)
        {
            return Divide(lhs, rhs);
        }
        #endregion

        public static Fixed operator *(Fixed2 lhs, Fixed2 rhs)
        {
            return Dot(lhs, rhs);
        }

        public static bool operator ==(Fixed2 lhs, Fixed2 rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Fixed2 lhs, Fixed2 rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion
        #endregion
    }
}
