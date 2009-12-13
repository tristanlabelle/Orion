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
        private readonly Func<UnitType, bool> predicate;
        private readonly UnitStat stat;
        private readonly int value;
        #endregion

        #region Constructors
        public TechnologyEffect(Func<UnitType, bool> predicate, UnitStat stat, int value)
        {
            Argument.EnsureNotNull(predicate, "predicate");
            Argument.EnsureDefined(stat, "stat");
            Argument.EnsureNotEqual(value, 0, "value");

            this.predicate = predicate;
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
        public bool AppliesTo(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return predicate(unitType);
        }

        public override string ToString()
        {
            return "+{0} {1}".FormatInvariant(value, stat);
        }
        #endregion
    }
}
