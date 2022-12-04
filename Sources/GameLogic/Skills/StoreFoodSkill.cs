using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to store food.
    /// </summary>
    [Serializable]
    public sealed class StoreFoodSkill : Skill
    {
        #region Fields
        private readonly int capacity;
        #endregion

        #region Constructors
        public StoreFoodSkill(int capacity)
        {
            Argument.EnsureStrictlyPositive(capacity, "capacity");

            this.capacity = capacity;
        }
        #endregion

        #region Properties
        public int Capacity
        {
            get { return capacity; }
        }
        #endregion

        #region Methods
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.FoodStorageCapacity) return capacity;
            return null;
        }
        #endregion
    }
}
