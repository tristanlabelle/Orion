using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Orion
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4 * sizeof(int))]
    public struct Region
    {
        #region Instance
        #region Fields
        private readonly int minX;
        private readonly int minY;
        private readonly int width;
        private readonly int height;
        #endregion

        #region Constructors
        public Region(int minX, int minY, int width, int height)
        {
            Argument.EnsurePositive(minX, "minX");
            Argument.EnsurePositive(minY, "minY");
            Argument.EnsurePositive(width, "width");
            Argument.EnsurePositive(height, "height");

            this.minX = minX;
            this.minY = minY;
            this.width = width;
            this.height = height;
        }
        #endregion

        #region Properties
        #region Min
        public int MinX
        {
            get { return minX; }
        }

        public int MinY
        {
            get { return minY; }
        }
        #endregion

        #region Size
        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }
        #endregion

        #region ExclusiveMax
        public int ExclusiveMaxX
        {
            get { return minX + width; }
        }

        public int ExclusiveMaxY
        {
            get { return minY + height; }
        }
        #endregion

        public int Area
        {
            get { return width * height; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "({0}, {1}) {2}x{3}".FormatInvariant(minX, minY, width, height);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        #endregion

        #region Methods
        #region Factory
        public static Region FromMinExclusiveMax(int minX, int minY, int exclusiveMaxX, int exclusiveMaxY)
        {
            return new Region(minX, minY, exclusiveMaxX - minX, exclusiveMaxY - minY);
        }

        public static Region Union(Region a, Region b)
        {
            return FromMinExclusiveMax(
                Math.Min(a.minX, b.minX), Math.Min(a.minY, b.minY),
                Math.Max(a.ExclusiveMaxX, b.ExclusiveMaxX),
                Math.Max(a.ExclusiveMaxY, b.ExclusiveMaxY));
        }
        #endregion
        #endregion
        #endregion
    }
}
