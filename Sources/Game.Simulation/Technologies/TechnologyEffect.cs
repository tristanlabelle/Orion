using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Components;
using System.Text;

namespace Orion.Game.Simulation.Technologies
{
    /// <summary>
    /// Represents one effect that a technology has.
    /// </summary>
    [Serializable]
    public struct TechnologyEffect
    {
        #region Fields
        private readonly Stat stat;
        private readonly StatValue delta;
        #endregion

        #region Constructors
        public TechnologyEffect(Stat stat, StatValue delta)
        {
            Argument.EnsureNotNull(stat, "stat");

            this.stat = stat;
            this.delta = delta;

            Debug.Assert((float)delta != 0, "Technology effect has a change of zero and will have no effect.");
            Debug.Assert(stat != Vision.RangeStat
                && stat != FactionMembership.ProvidedFoodStat
                && stat != Cost.FoodStat,
                "Technology effects changing stat {0} are not supported, they would cause bugs.".FormatInvariant(stat));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Stat"/> affected by this effect.
        /// </summary>
        public Stat Stat
        {
            get { return stat; }
        }

        /// <summary>
        /// Gets the amount of change this effect has on the stat.
        /// </summary>
        public StatValue Delta
        {
            get { return delta; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((float)delta >= 0) stringBuilder.Append('+');
            stringBuilder.Append((float)delta);
            stringBuilder.Append(' ');
            stringBuilder.Append(stat.FullName);
            return stringBuilder.ToString();
        }
        #endregion
    }
}
