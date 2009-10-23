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
        AlageneCost, // [0, infinity
        AladdiumCost, // [0, infinity
        AttackRange, // [0, infinity
        AttackPower, // [0, infinity
        AttackSpeed, // [0, infinity
        CreationSpeed, // [0, infinity
        MaxHealth, // [1, infinity
        MovementSpeed, // [0, infinity
        SightRange // [1, infinity
    }
}
