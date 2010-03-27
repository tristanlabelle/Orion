using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// An exception that is thrown when an attempt was made to query a stat on
    /// a skill to which it is not associated.
    /// </summary>
    [Serializable]
    public sealed class InvalidSkillStatException : ArgumentException
    {
        #region Fields
        private readonly Type skillType;
        private readonly UnitStat stat;
        #endregion

        #region Constructors
        public InvalidSkillStatException(Type skillType, UnitStat stat)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            Argument.EnsureNotNull(stat, "stat");

            this.skillType = skillType;
            this.stat = stat;
        }
        #endregion

        #region Properties
        public Type SkillType
        {
            get { return skillType; }
        }

        public UnitStat Stat
        {
            get { return stat; }
        }

        public override string ParamName
        {
            get { return "stat"; }
        }

        public override string Message
        {
            get { return "Invalid stat {0} for {1}.".FormatInvariant(stat, skillType.Name); }
        }
        #endregion

        #region Methods
        #endregion
    }
}
