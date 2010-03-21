using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Orion.Engine
{
    /// <summary>
    /// Provides functionality to represent and make operations on colors.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4 * sizeof(float))]
    [Serializable]
    [ImmutableObject(true)]
    public struct ColorRgba : IEquatable<ColorRgba>
    {
        #region Instance
        #region Fields
        /// <summary>
        /// The red component of this color.
        /// </summary>
        public readonly float R;

        /// <summary>
        /// The green component of this color.
        /// </summary>
        public readonly float G;

        /// <summary>
        /// The blue component of this color.
        /// </summary>
        public readonly float B;

        /// <summary>
        /// The alpha component of this color.
        /// </summary>
        public readonly float A;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new color from red, green, blue and alpha components
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <param name="alpha">The alpha component.</param>
        public ColorRgba(float red, float green, float blue, float alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        /// <summary>
        /// Constructs a new opaque color from red, green and blue components
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public ColorRgba(float red, float green, float blue)
            : this(red, green, blue, 1) { }

        public ColorRgba(ColorRgb color)
            : this(color.R, color.G, color.B) { }

        public ColorRgba(ColorRgb color, float alpha)
            : this(color.R, color.G, color.B, alpha) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the luminance of this color with human eye calibration.
        /// </summary>
        public float Luminance
        {
            get { return 0.3f * R + 0.59f * G + 0.11f * B; }
        }

        /// <summary>
        /// Gets a value indicating if a color is opaque.
        /// </summary>
        public bool IsOpaque
        {
            get { return A >= 0.9999f; }
        }

        /// <summary>
        /// Gets the RGB components of this color.
        /// </summary>
        public ColorRgb Rgb
        {
            get { return new ColorRgb(R, G, B); }
        }

        #region Bytes
        /// <summary>
        /// Gets the red component of this color as a byte.
        /// </summary>
        public byte ByteR
        {
            get { return FloatToByte(R); }
        }

        /// <summary>
        /// Gets the green component of this color as a byte.
        /// </summary>
        public byte ByteG
        {
            get { return FloatToByte(G); }
        }

        /// <summary>
        /// Gets the blue component of this color as a byte.
        /// </summary>
        public byte ByteB
        {
            get { return FloatToByte(B); }
        }

        /// <summary>
        /// Gets the alpha component of this color as a byte.
        /// </summary>
        public byte ByteA
        {
            get { return FloatToByte(A); }
        }
        #endregion
        #endregion

        #region Methods
        #region ToArray
        #region With Alpha
        /// <summary>
        /// Converts a color to a 4-element array containing the RGBA components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayRgba()
        {
            return new[] { R, G, B, A };
        }

        /// <summary>
        /// Converts a color to a 4-element array containing the ARGB components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayArgb()
        {
            return new[] { A, R, G, B };
        }

        /// <summary>
        /// Converts a color to a 4-element array containing the BGRA components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayBgra()
        {
            return new[] { B, G, R, A };
        }

        /// <summary>
        /// Converts a color to a 4-element array containing the ABGR components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayAbgr()
        {
            return new[] { A, B, G, R };
        }
        #endregion

        #region Without Alpha
        /// <summary>
        /// Converts a color to a 3-element array containing the RGB components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayRgb()
        {
            return new[] { R, G, B };
        }

        /// <summary>
        /// Converts a color to a 3-element array containing the BGR components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArrayBgr()
        {
            return new[] { B, G, R };
        }
        #endregion
        #endregion

        #region Object Model
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(ColorRgba other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ColorRgba)) return false;
            return Equals((ColorRgba)obj);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        public override string ToString()
        {
            return "[{0}, {1}, {2}, {3}]".FormatInvariant(R, G, B, A);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        #region Named Constructors
        /// <summary>
        /// Constructs a new color RGBA from its components stored as bytes.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The transparency component of the color.</param>
        /// <returns>A newly created color with the corresponding values.</returns>
        public static ColorRgba FromBytes(byte red, byte green, byte blue, byte alpha)
        {
            return new ColorRgba(ByteToFloat(red), ByteToFloat(green), ByteToFloat(blue), ByteToFloat(alpha));
        }

        /// <summary>
        /// Constructs a new color RGB from its components stored as bytes with the
        /// transparency component considered as 1.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <returns>A newly created color with the corresponding values.</returns>
        public static ColorRgba FromBytes(byte red, byte green, byte blue)
        {
            return new ColorRgba(ByteToFloat(red), ByteToFloat(green), ByteToFloat(blue));
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(ColorRgba a, ColorRgba b)
        {
            return a.Equals(b);
        }
        #endregion

        #region Arithmetic
        /// <summary>
        /// Gets a new color that corresponds to a color with all components clamped in unit range.
        /// </summary>
        /// <param name="color">The original color.</param>
        /// <returns>The resulting clamped color.</returns>
        public static ColorRgba Clamp(ColorRgba color)
        {
            return new ColorRgba(ClampToUnit(color.R), ClampToUnit(color.G), ClampToUnit(color.B), ClampToUnit(color.A));
        }

        private static float ClampToUnit(float value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color.</param>
        /// <param name="amount">A value indicating the location of the result between the two colors.</param>
        /// <returns>The resulting interpolated color.</returns>
        public static ColorRgba Lerp(ColorRgba first, ColorRgba second, float amount)
        {
            return new ColorRgba(
                Lerp(first.R, second.R, amount),
                Lerp(first.G, second.G, amount),
                Lerp(first.B, second.B, amount),
                Lerp(first.A, second.A, amount));
        }

        private static float Lerp(float first, float second, float progress)
        {
            return first + (second - first) * progress;
        }

        /// <summary>
        /// Premultiplies the RGB components of a color with its alpha component.
        /// </summary>
        /// <param name="color">The color to be alpha premultiplied.</param>
        /// <returns>The resulting alpha premultiplied color.</returns>
        public static ColorRgba PremultiplyAlpha(ColorRgba color)
        {
            return new ColorRgba(color.Rgb * color.A, color.A);
        }

        /// <summary>
        /// Adds two colors component-wise.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgba Add(ColorRgba first, ColorRgba second)
        {
            return new ColorRgba(first.R + second.R, first.G + second.G, first.B + second.B, first.A + second.A);
        }

        /// <summary>
        /// Substract two colors component-wise
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgba Substract(ColorRgba first, ColorRgba second)
        {
            return new ColorRgba(first.R - second.R, first.G - second.G, first.B - second.B, first.A - second.A);
        }

        /// <summary>
        /// Multiplies a color by a scalar component-wise.
        /// </summary>
        /// <param name="color">The color to be multiplied.</param>
        /// <param name="multiplier">The multiplying factor.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgba Multiply(ColorRgba color, float multiplier)
        {
            return new ColorRgba(color.R * multiplier, color.G * multiplier,
                color.B * multiplier, color.A * multiplier);
        }

        /// <summary>
        /// Multiplies a scalar by a color, component-wise.
        /// </summary>
        /// <param name="multiplier">The multiplying factor.</param>
        /// <param name="color">The color to be multiplied.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgba Multiply(float multiplier, ColorRgba color)
        {
            return Multiply(color, multiplier);
        }

        /// <summary>
        /// Performs a component-wise product of two colows.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting product of the colors.</returns>
        public static ColorRgba MultiplyComponents(ColorRgba first, ColorRgba second)
        {
            return new ColorRgba(first.R * second.R, first.G * second.G, first.B * second.B, first.A * second.A);
        }

        /// <summary>
        /// Divides a color by a value component-wise.
        /// </summary>
        /// <param name="color">The color to be divided.</param>
        /// <param name="divider">The divising factor.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgba Divide(ColorRgba color, float divider)
        {
            return Multiply(color, 1.0f / divider);
        }
        #endregion

        #region Helpers
        private static byte FloatToByte(float value)
        {
            if (value < 0) return 0;
            else if (value > 1) return 255;
            return (byte)(value * 255.0f);
        }

        private static float ByteToFloat(byte value)
        {
            return (float)value / 255.0f;
        }
        #endregion
        #endregion

        #region Operators
        #region Casting
        public static implicit operator ColorRgba(ColorRgb color)
        {
            return color.ToRgba();
        }

        public static explicit operator ColorRgb(ColorRgba color)
        {
            return color.Rgb;
        }
        #endregion

        #region Arithmetic
        public static ColorRgba operator +(ColorRgba lhs, ColorRgba rhs)
        {
            return Add(lhs, rhs);
        }

        public static ColorRgba operator -(ColorRgba lhs, ColorRgba rhs)
        {
            return Substract(lhs, rhs);
        }

        public static ColorRgba operator *(ColorRgba color, float multiplier)
        {
            return Multiply(color, multiplier);
        }

        public static ColorRgba operator *(float multiplier, ColorRgba color)
        {
            return Multiply(multiplier, color);
        }

        public static ColorRgba operator *(ColorRgba lhs, ColorRgba rhs)
        {
            return MultiplyComponents(rhs, lhs);
        }

        public static ColorRgba operator /(ColorRgba color, float divider)
        {
            return Divide(color, divider);
        }
        #endregion

        #region Comparison
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(ColorRgba a, ColorRgba b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(ColorRgba a, ColorRgba b)
        {
            return !Equals(a, b);
        }
        #endregion
        #endregion
        #endregion
    }
}
