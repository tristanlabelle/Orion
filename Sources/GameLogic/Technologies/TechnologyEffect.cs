using System;

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
        private readonly int value;
        #endregion

        #region Constructors
        public TechnologyEffect(UnitStat stat, int value)
        {
            Argument.EnsureNotNull(stat, "stat");
            Argument.EnsureNotEqual(value, 0, "value");

            this.stat = stat;
            this.value = value;
        }
        #endregion

        #region Properties
        public UnitStat Stat
        {
            get { return stat; }
        }

        public int Value
        {
            get { return value; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "+{0} {1}".FormatInvariant(value, stat);
        }
        #endregion
    }
}
