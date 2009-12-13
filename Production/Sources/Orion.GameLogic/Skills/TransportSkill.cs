using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> permitting a <see cref="Unit"/> to transport other <see cref="Unit"/>s.
    /// </summary>
    public sealed class TransportSkill : Skill
    {
        #region Fields
        private readonly int capacity;
        #endregion

        #region Constructors
        public TransportSkill(int capacity)
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
            if (stat == UnitStat.TransportCapacity) return capacity;
            return null;
        }
        #endregion
    }
}
