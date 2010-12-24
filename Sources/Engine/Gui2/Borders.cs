using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Stores a thickness value along four edges.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct Borders : IEquatable<Borders>
    {
        #region Instance
        #region Fields
        public readonly int MinX;
        public readonly int MinY;
        public readonly int MaxX;
        public readonly int MaxY;
        #endregion

        #region Constructors
        public Borders(int minX, int minY, int maxX, int maxY)
        {
            Argument.EnsurePositive(minX, "minX");
            Argument.EnsurePositive(minY, "minY");
            Argument.EnsurePositive(maxX, "maxX");
            Argument.EnsurePositive(maxY, "maxY");

            this.MinX = minX;
            this.MinY = minY;
            this.MaxX = maxX;
            this.MaxY = maxY;
        }

        public Borders(int x, int y)
        {
            Argument.EnsurePositive(x, "x");
            Argument.EnsurePositive(y, "y");

            this.MinX = x;
            this.MinY = y;
            this.MaxX = x;
            this.MaxY = y;
        }

        public Borders(int amount)
        {
            Argument.EnsurePositive(amount, "amount");

            this.MinX = amount;
            this.MinY = amount;
            this.MaxX = amount;
            this.MaxY = amount;
        }
        #endregion

        #region Properties
        public int TotalX
        {
            get { return MinX + MaxX; }
        }

        public int TotalY
        {
            get { return MinY + MaxY; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests for equality with another instance.
        /// </summary>
        /// <param name="other">The instance to be tested with.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public bool Equals(Borders other)
        {
            return MinX == other.MinX && MinY == other.MinY
                && MaxX == other.MaxX && MaxY == other.MaxY;
        }

        public override bool Equals(object obj)
        {
            return obj is Borders && Equals((Borders)obj);
        }

        public override int GetHashCode()
        {
            return (MinX << 21) ^ (MinY << 14) ^ (MaxX << 7) ^ MaxY;
        }

        public override string ToString()
        {
            if (MinX == MaxX && MinY == MaxY)
            {
                if (MinX == MinY) return MinX.ToStringInvariant();
                return "({0} {1})".FormatInvariant(MinX, MinY);
            }

            return "({0} {1} {2} {3})".FormatInvariant(MinX, MinY, MaxX, MaxY);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool Equals(Borders a, Borders b)
        {
            return a.Equals(b);
        }

        public static Region Grow(Region rectangle, Borders borders)
        {
            return new Region(
                rectangle.MinX - borders.MinX, rectangle.MinY - borders.MinY,
                rectangle.Width + borders.TotalX, rectangle.Height + borders.TotalY);
        }

        public static Region? Shrink(Region? rectangle, Borders borders)
        {
            Region result;
            return rectangle.HasValue && TryShrink(rectangle.Value, borders, out result) ? result : (Region?)null;
        }

        public static bool TryShrink(Region rectangle, Borders borders, out Region result)
        {
            if (borders.TotalX > rectangle.Width || borders.TotalY > rectangle.Height)
            {
                result = default(Region);
                return false;
            }

            result = new Region(
                rectangle.MinX + borders.MinX, rectangle.MinY + borders.MinY,
                rectangle.Width - borders.TotalX, rectangle.Height - borders.TotalY);
            return true;
        }
        #endregion

        #region Operators
        public static implicit operator Borders(int value)
        {
            return new Borders(value);
        }

        public static implicit operator Borders(Size size)
        {
            return new Borders(size.Width, size.Height);
        }

        /// <summary>
        /// Tests two instances for equality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are equal, false otherwise.</returns>
        public static bool operator ==(Borders a, Borders b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Tests two instances for inequality.
        /// </summary>
        /// <param name="a">The first instance.</param>
        /// <param name="b">The second instance.</param>
        /// <returns>True they are different, false otherwise.</returns>
        public static bool operator !=(Borders a, Borders b)
        {
            return !Equals(a, b);
        }

        public static Region operator +(Region rectangle, Borders borders)
        {
            return Grow(rectangle, borders);
        }

        public static Region? operator -(Region? rectangle, Borders borders)
        {
            return Shrink(rectangle, borders);
        }

        public static Size operator +(Size size, Borders borders)
        {
            return new Size(size.Width + borders.TotalX, size.Height + borders.TotalY);
        }

        public static Size? operator -(Size? size, Borders borders)
        {
            if (!size.HasValue) return null;

            Size sizeValue = size.Value;
            if (borders.TotalX > sizeValue.Width || borders.TotalY > sizeValue.Height) return null;

            return new Size(sizeValue.Width - borders.TotalX, sizeValue.Height - borders.TotalY);
        }
        #endregion
        #endregion
    }
}
