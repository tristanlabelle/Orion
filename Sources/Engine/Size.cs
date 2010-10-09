using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.InteropServices;
using OpenTK;
using System.Globalization;

namespace Orion.Engine
{
    /// <summary>
    /// Represents the 2D integral dimensions of an object.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    [StructLayout(LayoutKind.Sequential, Size = sizeof(int) * 2)]
    public struct Size : IEquatable<Size>, IFormattable
    {
        #region Instance
        #region Fields
        public readonly int Width;
        public readonly int Height;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Size"/> from a width and a height.
        /// </summary>
        /// <param name="width">The width of the size.</param>
        /// <param name="height">The height of the size.</param>
        public Size(int width, int height)
        {
            Argument.EnsurePositive(width, "width");
            Argument.EnsurePositive(height, "height");
            this.Width = width;
            this.Height = height;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the area of this size.
        /// </summary>
        public int Area
        {
            get { return Width * Height; }
        }
        #endregion

        #region Methods
        public Vector2 ToVector()
        {
            return new Vector2(Width, Height);
        }

        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(Size other)
        {
            return Width == other.Width && Height == other.Height;
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

        /// <summary>
        /// Converts this <see cref="Size"/> to a string using specific culture information.
        /// </summary>
        /// <param name="formatProvider">The format provider to be used.</param>
        /// <returns>A string representation of this <see cref="Size"/>.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "{0}x{1}", Width, Height);
        }
        #endregion

        #region Explicit Members
        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (format != null) throw new NotSupportedException("Format strings are not supported by Size objects.");
            return ToString(formatProvider);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A size with a width and a height of zero.
        /// </summary>
        public static readonly Size Zero = new Size(0, 0);

        private static readonly Regex parsingRegex
            = new Regex(@"\A\s*([0-9]+)x([0-9]+)\s*\Z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        #endregion

        #region Methods
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(Size a, Size b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Parses a Size represented as a <see cref="string"/> with the format "{0}x{1}".
        /// </summary>
        /// <param name="size">The formatted string to be parsed.</param>
        /// <returns>The resulting parsed size.</returns>
        public static Size Parse(string sizeString)
        {
            Match result = parsingRegex.Match(sizeString);
            if (!result.Success) throw new FormatException("Required format for a Size object is {{Width}}x{{Height}}; got {0}.".FormatInvariant(sizeString));

            int width = int.Parse(result.Groups[1].Value, NumberFormatInfo.InvariantInfo);
            int height = int.Parse(result.Groups[2].Value, NumberFormatInfo.InvariantInfo);
            return new Size(width, height);
        }

        public static bool TryParse(string sizeString, out Size into)
        {
            try
            {
                into = Parse(sizeString);
                return true;
            }
            catch (FormatException)
            {
                into = default(Size);
                return false;
            }
        }
        #endregion

        #region Operators
        public static bool operator ==(Size a, Size b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Size a, Size b)
        {
            return !Equals(a, b);
        }

        public static explicit operator Vector2(Size size)
        {
            return size.ToVector();
        }
        #endregion
        #endregion
    }
}
