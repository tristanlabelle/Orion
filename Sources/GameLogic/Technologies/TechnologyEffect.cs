using System;
using System.Diagnostics;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Represents one effect that a technology has.
    /// </summary>
    [Serializable]
    public struct TechnologyEffect
    {
        #region Fields
        private readonly UnitStat stat;
        private readonly int change;
        #endregion

        #region Constructors
        public TechnologyEffect(UnitStat stat, int change)
        {
            Argument.EnsureNotNull(stat, "stat");

            this.stat = stat;
            this.change = change;

            Debug.Assert(change != 0, "Technology effect has a change of zero and will have no effect.");
            Debug.Assert(stat != UnitStat.SightRange && stat != UnitStat.StoreFoodCapacity && stat != UnitStat.FoodCost,
                "Technology effects changing stat {0} are not supported, they would cause bugs.".FormatInvariant(stat));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the stat affected by this effect.
        /// </summary>
        public UnitStat Stat
        {
            get { return stat; }
        }

        /// <summary>
        /// Gets the change this effect makes to the stat.
        /// </summary>
        public int Change
        {
            get { return change; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "{0} {1}".FormatInvariant(
                change >= 0 ? "+" + change.ToStringInvariant() : change.ToStringInvariant(),
                stat);
        }
        #endregion
    }
}
