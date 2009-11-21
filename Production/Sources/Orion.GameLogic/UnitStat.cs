using System;

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
        AttackDelay,
        BuildingSpeed,
        ExtractingSpeed,
        MaxCarryingAmount,
        MaxHealth,
        MovementSpeed,
        SightRange,
        TrainingSpeed,
        FoodStorageCapacity, 
        CanFly
    }
}
