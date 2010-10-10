using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation
{
    [Serializable]
    [Flags]
    public enum DiplomaticStance
    {
        Enemy           = 0,
        SharedVision    = (1 << 0),
        AlliedVictory   = (1 << 1),
        SharedControl   = (1 << 2),
        ForeverAllied   = SharedVision | AlliedVictory | SharedControl
    }

    public static class DiplomaticStanceExtensionMethods
    {
        public static bool HasFlag(this DiplomaticStance stance, DiplomaticStance flag)
        {
            return ((long)stance & (long)flag) == (long)flag;
        }

        public static DiplomaticStance Exclude(this DiplomaticStance stance, DiplomaticStance flag)
        {
            return (DiplomaticStance)((long)stance & ~(long)flag);
        }
    }
}
