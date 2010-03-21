using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Provides extension methods to numeric types.
    /// </summary>
    public static class NumericExtensions
    {
        public static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
