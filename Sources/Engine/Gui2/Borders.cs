using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public struct Borders
    {
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
        #endregion
        
        #region Operators
        public static Region operator +(Region rectangle, Borders borders)
        {
            return new Region(
                rectangle.MinX - borders.MinX, rectangle.MinY - borders.MinY,
                rectangle.Width + borders.TotalX, rectangle.Height + borders.TotalY);
        }

        public static Region? operator -(Region? rectangle, Borders borders)
        {
            if (!rectangle.HasValue) return null;

            Region rectangleValue = rectangle.Value;
            if (borders.TotalX > rectangleValue.Width || borders.TotalY > rectangleValue.Height) return null;

            return new Region(
                rectangleValue.MinX + borders.MinX, rectangleValue.MinY + borders.MinY,
                rectangleValue.Width - borders.TotalX, rectangleValue.Height - borders.TotalY);
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
    }
}
