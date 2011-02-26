using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// Identifies a stat associated with a unit.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class UnitStat
    {
        #region Fields
        private readonly Type skillType;
        private readonly string skillName;
        private readonly string name;
        private readonly string fullName;
        #endregion

        #region Constructors
        internal UnitStat(Type skillType, string name)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            Argument.EnsureNotNull(name, "name");

            this.skillType = skillType;
            this.skillName = UnitSkill.GetTypeName(skillType);
            this.name = name;
            this.fullName = skillName + '.' + name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the skill which defines this stat.
        /// </summary>
        public Type SkillType
        {
            get { return skillType; }
        }

        /// <summary>
        /// Gets the name of the skill which defines this stat.
        /// </summary>
        public string SkillName
        {
            get { return skillName; }
        }

        /// <summary>
        /// Gets the name of this stat within its skill.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the fully qualified name of this stat.
        /// </summary>
        public string FullName
        {
            get { return fullName; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return fullName;
        }
        #endregion
    }
}
