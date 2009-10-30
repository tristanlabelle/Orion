using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Identifies a stat associated with a unit.
    /// </summary>
    [Serializable]
    public enum UnitStat
    {
        AlageneCost,
        AladdiumCost,
        AttackRange,
        AttackPower,
        BuildingSpeed,
        MaxHealth,
        MovementSpeed,
        SightRange
    }
}
