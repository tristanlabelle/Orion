using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine
{
    public static class EnumFlagsExtensionMethods
    {
        public static bool HasFlag(this Enum enumValue, Enum flag)
        {
            long flagAsLong = Convert.ToInt64(flag);
            return (Convert.ToInt64(enumValue) & flagAsLong) == flagAsLong;
        }
    }
}
