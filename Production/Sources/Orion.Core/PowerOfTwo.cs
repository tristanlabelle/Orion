using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    public static class PowerOfTwo
    {
        public static uint Ceiling(uint value)
        {
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        public static bool Is(uint value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }
    }
}
