using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Orion.Engine
{
    /// <summary>
    /// Represents 32 bits fixed-point numbers in Q16.16 format.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    [ImmutableObject(true)]
    public struct Fixed : IEquatable<Fixed>, IComparable, IComparable<Fixed>, IFormattable
    {
        #region Instance
        #region Fields
        private readonly int rawValue;
        #endregion

        #region Constructors
        private Fixed(int rawValue)
        {
            this.rawValue = rawValue;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the internal representation of the fixed-point number.
        /// </summary>
        public int RawValue
        {
            get { return rawValue; }
        }
        #endregion

        #region Methods
        #region Conversions
        public int ToInt32()
        {
            return rawValue >> FractionalBitCount;
        }

        public long ToInt64()
        {
            return rawValue >> FractionalBitCount;
        }

        public float ToSingle()
        {
            return rawValue / (float)One.rawValue;
        }

        public double ToDouble()
        {
            return rawValue / (double)One.rawValue;
        }
        #endregion

        #region Comparison/Equality
        public int CompareTo(Fixed other)
        {
            return rawValue.CompareTo(other.rawValue);
        }

        public bool Equals(Fixed other)
        {
            return rawValue == other.rawValue;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixed && Equals((Fixed)obj);
        }

        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }
        #endregion

        #region ToString
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToDouble().ToString(format, formatProvider);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }
        
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }
        #endregion
        #endregion

        #region Explicit Members
        int IComparable.CompareTo(object obj)
        {
 	        if (!(obj is Fixed)) throw new ArgumentException("Cannot compare with non-Fixed object.", "obj");
            return CompareTo((Fixed)obj);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// The number of bits representing the integral part of the number.
        /// </summary>
        public const int IntegralBitCount = 16;

        /// <summary>
        /// The number of bits representing the fractional part of the number.
        /// </summary>
        public const int FractionalBitCount = 16;

        /// <summary>
        /// The bit mask for the fractional part of the raw value.
        /// </summary>
        public const int FractionalBitMask = (1 << FractionalBitCount) - 1;

        /// <summary>
        /// The bit mask for the integral part of the raw value.
        /// </summary>
        public const int IntegralBitMask = ~FractionalBitMask;

        /// <summary>
        /// A fixed-point number with a value of zero.
        /// </summary>
        public static readonly Fixed Zero = FromRawValue(0);

        /// <summary>
        /// A fixed-point number with a value of one.
        /// </summary>
        public static readonly Fixed One = FromRawValue(1 << FractionalBitCount);

        /// <summary>
        /// A fixed-point number with a value of a half.
        /// </summary>
        public static readonly Fixed Half = FromRawValue(1 << (FractionalBitCount - 1));

        /// <summary>
        /// A fixed-point number with the smallest fractional value.
        /// </summary>
        public static readonly Fixed Epsilon = FromRawValue(1);

        /// <summary>
        /// A fixed-point number with the greatest representable value.
        /// </summary>
        public static readonly Fixed MaxValue = FromRawValue(int.MaxValue);

        /// <summary>
        /// A fixed-point number with the lowest representable value.
        /// </summary>
        public static readonly Fixed MinValue = FromRawValue(int.MinValue);

        /// <summary>
        /// A fixed-point number with a value of PI.
        /// </summary>
        public static readonly Fixed PI = FromRawValue(205887);

        /// <summary>
        /// A fixed-point number with a value of E.
        /// </summary>
        public static readonly Fixed E = FromRawValue(178145);
        #endregion

        #region Methods
        #region Factory Methods
        public static Fixed FromRawValue(int value)
        {
            return FromRawValue(value);
        }

        public static Fixed FromInt32(int value)
        {
            return FromRawValue(value << FractionalBitCount);
        }

        public static Fixed FromInt64(long value)
        {
            return FromInt32((int)value);
        }

        public static Fixed FromSingle(float value)
        {
            return FromRawValue((int)(value * One.rawValue));
        }

        public static Fixed FromDouble(double value)
        {
            return FromRawValue((int)(value * One.rawValue));
        }
        #endregion

        #region Arithmetic
        #region Additive
        public static Fixed Negate(Fixed value)
        {
            return FromRawValue(-value.rawValue);
        }

        public static Fixed Add(Fixed term1, Fixed term2)
        {
            return FromRawValue(term1.rawValue + term2.rawValue);
        }

        public static Fixed Subtract(Fixed term1, Fixed term2)
        {
            return FromRawValue(term1.rawValue - term2.rawValue);
        }
        #endregion

        #region Multiplicative
        public static Fixed Multiply(Fixed term1, Fixed term2)
        {
            long scaledProduct = (long)term1.rawValue * (long)term2.rawValue;
            return FromRawValue((int)(scaledProduct >> FractionalBitCount));
        }

        public static Fixed Divide(Fixed term1, Fixed term2)
        {
            long result = ((long)term1.rawValue << FractionalBitCount) / term2.rawValue;
            return FromRawValue((int)result);
        }

        public static Fixed Multiply(Fixed term1, int term2)
        {
            return FromRawValue(term1.rawValue * term2);
        }

        public static Fixed Divide(Fixed term1, int term2)
        {
            return FromRawValue(term1.rawValue / term2);
        }

        public static Fixed Mod(Fixed term1, Fixed term2)
        {
            return FromRawValue(term1.rawValue % term2.rawValue);
        }
        #endregion

        #region Binary
        public static Fixed LeftShift(Fixed term, int shift)
        {
            return FromRawValue(term.rawValue << shift);
        }

        public static Fixed RightShift(Fixed term, int shift)
        {
            return FromRawValue(term.rawValue >> shift);
        }
        #endregion
        #endregion

        #region Math Utility
        public static int Sign(Fixed value)
        {
            return Math.Sign(value.rawValue);
        }

#warning Fixed-point operations should not depend on System.Math floating-point operations.
        public static Fixed Abs(Fixed value)
        {
            return FromRawValue(Math.Abs(value.rawValue));
        }

        public static Fixed Sqrt(Fixed value)
        {
            return (Fixed)Math.Sqrt((double)value);
        }

        public static Fixed Sin(Fixed value)
        {
            return (Fixed)Math.Sin((double)value);
        }

        public static Fixed Cos(Fixed value)
        {
            return (Fixed)Math.Cos((double)value);
        }

        public static Fixed Tan(Fixed value)
        {
            return (Fixed)Math.Tan((double)value);
        }

        public static Fixed Atan2(Fixed y, Fixed x)
        {
            return (Fixed)Math.Atan2((double)y, (double)x);
        }

        public static Fixed Min(Fixed term1, Fixed term2)
        {
            return FromRawValue(Math.Min(term1.rawValue, term2.rawValue));
        }

        public static Fixed Max(Fixed term1, Fixed term2)
        {
            return FromRawValue(Math.Max(term1.rawValue, term2.rawValue));
        }

        public static Fixed Clamp(Fixed value, Fixed min, Fixed max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static Fixed Lerp(Fixed a, Fixed b, Fixed blend)
        {
            return a + (b - a) * blend;
        }

        public static Fixed Truncate(Fixed term)
        {
            if (term.rawValue >= 0)
            {
                return FromRawValue(term.rawValue & IntegralBitMask);
            }
            else
            {
                return FromRawValue((int)-((-(long)term.rawValue) & IntegralBitMask));
            }
        }

        public static Fixed Round(Fixed term)
        {
            if (term.rawValue >= 0)
            {
                return FromRawValue((term.rawValue + Half.rawValue) & IntegralBitMask);
            }
            else
            {
                return FromRawValue((int)-((-(long)term.rawValue + Half.rawValue) & IntegralBitMask));
            }
        }
        #endregion

        #region Object Model
        public static bool Equals(Fixed term1, Fixed term2)
        {
            return term1.Equals(term2);
        }

        public static int Compare(Fixed term1, Fixed term2)
        {
            return term1.CompareTo(term2);
        }
        #endregion
        #endregion

        #region Operators
        #region Arithmetic
        #region Additive
        public static Fixed operator +(Fixed term)
        {
            return term;
        }

        public static Fixed operator -(Fixed term)
        {
            return Negate(term);
        }

        public static Fixed operator +(Fixed lhs, Fixed rhs)
        {
            return Add(lhs, rhs);
        }

        public static Fixed operator -(Fixed lhs, Fixed rhs)
        {
            return Subtract(lhs, rhs);
        }
        #endregion

        #region Multiplicative
        public static Fixed operator *(Fixed lhs, Fixed rhs)
        {
            return Multiply(lhs, rhs);
        }

        public static Fixed operator /(Fixed lhs, Fixed rhs)
        {
            return Divide(lhs, rhs);
        }

        public static Fixed operator *(Fixed lhs, int rhs)
        {
            return Multiply(lhs, rhs);
        }

        public static Fixed operator /(Fixed lhs, int rhs)
        {
            return Divide(lhs, rhs);
        }

        public static Fixed operator *(int lhs, Fixed rhs)
        {
            return Multiply(rhs, lhs);
        }

        public static Fixed operator %(Fixed lhs, Fixed rhs)
        {
            return Mod(lhs, rhs);
        }
        #endregion

        #region Binary
        public static Fixed operator <<(Fixed lhs, int rhs)
        {
            return LeftShift(lhs, rhs);
        }

        public static Fixed operator >>(Fixed lhs, int rhs)
        {
            return RightShift(lhs, rhs);
        }
        #endregion
        #endregion

        #region Equality
        public static bool operator ==(Fixed lhs, Fixed rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Fixed lhs, Fixed rhs)
        {
            return !Equals(lhs, rhs);
        }
        #endregion

        #region Comparison
        public static bool operator <(Fixed lhs, Fixed rhs)
        {
            return Compare(lhs, rhs) < 0;
        }

        public static bool operator <=(Fixed lhs, Fixed rhs)
        {
            return Compare(lhs, rhs) <= 0;
        }

        public static bool operator >(Fixed lhs, Fixed rhs)
        {
            return Compare(lhs, rhs) > 0;
        }

        public static bool operator >=(Fixed lhs, Fixed rhs)
        {
            return Compare(lhs, rhs) >= 0;
        }
        #endregion

        #region Casting To Other Types
        public static explicit operator int(Fixed value)
        {
            return value.ToInt32();
        }

        public static explicit operator long(Fixed value)
        {
            return value.ToInt64();
        }

        public static explicit operator float(Fixed value)
        {
            return value.ToSingle();
        }

        public static explicit operator double(Fixed value)
        {
            return value.ToDouble();
        }
        #endregion

        #region Casting From Other Types
        public static implicit operator Fixed(int value)
        {
            return FromInt32(value);
        }

        public static explicit operator Fixed(long value)
        {
            return FromInt64(value);
        }

        public static explicit operator Fixed(float value)
        {
            return FromSingle(value);
        }

        public static explicit operator Fixed(double value)
        {
            return FromDouble(value);
        }
        #endregion
        #endregion
        #endregion
    }
}
