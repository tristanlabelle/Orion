using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;

namespace Orion.Engine
{
    /// <summary>
    /// Provides functionality to represent and make operations on colors.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 3 * sizeof(float))]
    [Serializable]
    [ImmutableObject(true)]
    public struct ColorRgb : IEquatable<ColorRgb>
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
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new color from red, green, blue and alpha components
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public ColorRgb(float red, float green, float blue)
        {
            R = red;
            G = green;
            B = blue;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the luminance of this color with human eye calibration.
        /// </summary>
        public float HumanEyeLuminance
        {
            get { return 0.3f * R + 0.59f * G + 0.11f * B; }
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
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Converts a color to a 3-element array containing the RGB components.
        /// </summary>
        /// <returns>The array of color components.</returns>
        public float[] ToArray()
        {
            return new[] { R, G, B };
        }

        public ColorRgba ToRgba()
        {
            return new ColorRgba(this);
        }

        public ColorRgba ToRgba(float alpha)
        {
            return new ColorRgba(this, alpha);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
        }

        #region Object Model
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(ColorRgb other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ColorRgb)) return false;
            return Equals((ColorRgb)obj);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public override string ToString()
        {
            return "[{0}, {1}, {2}]".FormatInvariant(R, G, B);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Methods
        #region Factory
        /// <summary>
        /// Creates a new tint of gray.
        /// </summary>
        /// <param name="value">The intensity of the RGB color components.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgb CreateGray(float value)
        {
            return new ColorRgb(value, value, value);
        }

        /// <summary>
        /// Constructs a new color RGBA from its components stored as bytes.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The transparency component of the color.</param>
        /// <returns>A newly created color with the corresponding values.</returns>
        public static ColorRgb FromBytes(byte red, byte green, byte blue)
        {
            return new ColorRgb(ByteToFloat(red), ByteToFloat(green), ByteToFloat(blue));
        }
        #endregion

        #region Object Model
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="first">The first instance.</param>
        /// <param name="second">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(ColorRgb first, ColorRgb second)
        {
            return first.Equals(second);
        }
        #endregion

        #region Arithmetic
        /// <summary>
        /// Gets a new color that corresponds to a color with all components clamped in unit range.
        /// </summary>
        /// <param name="color">The original color.</param>
        /// <returns>The resulting clamped color.</returns>
        public static ColorRgb Clamp(ColorRgb color)
        {
            return new ColorRgb(ClampToUnit(color.R), ClampToUnit(color.G), ClampToUnit(color.B));
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
        public static ColorRgb Lerp(ColorRgb first, ColorRgb second, float amount)
        {
            return new ColorRgb(
                Lerp(first.R, second.R, amount),
                Lerp(first.G, second.G, amount),
                Lerp(first.B, second.B, amount));
        }

        private static float Lerp(float first, float second, float progress)
        {
            return first + (second - first) * progress;
        }

        /// <summary>
        /// Adds two colors component-wise.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgb Add(ColorRgb first, ColorRgb second)
        {
            return new ColorRgb(first.R + second.R, first.G + second.G, first.B + second.B);
        }

        /// <summary>
        /// Subtracts two colors component-wise
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgb Subtract(ColorRgb first, ColorRgb second)
        {
            return new ColorRgb(first.R - second.R, first.G - second.G, first.B - second.B);
        }

        /// <summary>
        /// Multiplies a color by a scalar component-wise.
        /// </summary>
        /// <param name="color">The color to be multiplied.</param>
        /// <param name="multiplier">The multiplying factor.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgb Multiply(ColorRgb color, float multiplier)
        {
            return new ColorRgb(color.R * multiplier, color.G * multiplier, color.B * multiplier);
        }

        /// <summary>
        /// Performs a component-wise product of two colows.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color</param>
        /// <returns>The resulting product of the colors.</returns>
        public static ColorRgb MultiplyComponents(ColorRgb first, ColorRgb second)
        {
            return new ColorRgb(first.R * second.R, first.G * second.G, first.B * second.B);
        }

        /// <summary>
        /// Divides a color by a value component-wise.
        /// </summary>
        /// <param name="color">The color to be divided.</param>
        /// <param name="divider">The divising factor.</param>
        /// <returns>The resulting color.</returns>
        public static ColorRgb Divide(ColorRgb color, float divider)
        {
            return Multiply(color, 1.0f / divider);
        }
        #endregion

        #region Helpers
        public static ColorRgb Deserialize(BinaryReader reader)
        {
            return new ColorRgb(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

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
        #region Arithmetic
        public static ColorRgb operator +(ColorRgb lhs, ColorRgb rhs)
        {
            return Add(lhs, rhs);
        }

        public static ColorRgb operator -(ColorRgb lhs, ColorRgb rhs)
        {
            return Subtract(lhs, rhs);
        }

        public static ColorRgb operator *(ColorRgb color, float multiplier)
        {
            return Multiply(color, multiplier);
        }

        public static ColorRgb operator *(float multiplier, ColorRgb color)
        {
            return Multiply(color, multiplier);
        }

        public static ColorRgb operator *(ColorRgb lhs, ColorRgb rhs)
        {
            return MultiplyComponents(rhs, lhs);
        }

        public static ColorRgb operator /(ColorRgb color, float divider)
        {
            return Divide(color, divider);
        }
        #endregion

        #region Comparison
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="first">The first instance.</param>
        /// <param name="second">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(ColorRgb first, ColorRgb second)
        {
            return Equals(first, second);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="first">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(ColorRgb first, ColorRgb second)
        {
            return !Equals(first, second);
        }
        #endregion
        #endregion
        #endregion
    }
}
