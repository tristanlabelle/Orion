using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> permitting a <see cref="Entity"/> to transport other <see cref="Entity"/>s.
    /// </summary>
    public sealed class TransportSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat CapacityStat = new UnitStat(typeof(TransportSkill), "Capacity");
        private int capacity = 1;
        #endregion

        #region Properties
        public int Capacity
        {
            get { return capacity; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Capacity");
                capacity = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new TransportSkill { capacity = capacity };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == CapacityStat) return capacity;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == CapacityStat) Capacity = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
