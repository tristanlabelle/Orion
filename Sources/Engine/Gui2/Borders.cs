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

        #region Methods
        #endregion
    }
}
