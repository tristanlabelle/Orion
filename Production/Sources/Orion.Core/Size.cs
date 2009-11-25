﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Orion
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
        private readonly int width;
        private readonly int height;
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
            this.width = width;
            this.height = height;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width component of this size.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height component of this size.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the area of this size.
        /// </summary>
        public int Area
        {
            get { return width * height; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(Size other)
        {
            return width == other.width && height == other.height;
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
            return string.Format(formatProvider, "{0}x{1}", width, height);
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
        #endregion
        #endregion
    }
}