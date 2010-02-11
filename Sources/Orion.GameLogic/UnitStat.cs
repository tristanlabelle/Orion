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
        MeleeArmor,
        RangedArmor,
        MovementSpeed,
        SightRange,
        TrainingSpeed,
        FoodStorageCapacity,
        HealSpeed,
        HealRange,
        TransportCapacity
    }
}
