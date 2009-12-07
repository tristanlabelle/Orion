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
        private readonly string tag;
        private readonly UnitStat stat;
        private readonly int value;
        #endregion

        #region Constructors
        public TechnologyEffect(string tag, UnitStat stat, int value)
        {
            Argument.EnsureNotNull(tag, "tag");
            Argument.EnsureDefined(stat, "stat");
            Argument.EnsureNotEqual(value, 0, "value");

            this.tag = tag;
            this.stat = stat;
            this.value = value;
        }
        #endregion

        #region Properties
        public string Tag
        {
            get { return tag; }
        }

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
            return "+{0} {1} for {2}".FormatInvariant(value, stat, tag);
        }
        #endregion
    }
}
