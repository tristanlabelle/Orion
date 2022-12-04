using System;
using System.Collections.Generic;
using System.Linq;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// Abstract base class for skills, which denotes the actions that can be done by units.
    /// </summary>
    [Serializable]
    public abstract class Skill
    {
        #region Instance
        #region Constructors
        internal Skill() { }
        #endregion

        #region Methods
        public virtual int? TryGetBaseStat(UnitStat stat) { return null; }
        #endregion
        #endregion

        #region Static
        #region Methods
        public static IEnumerable<Type> GetDependencies(Type skillType)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            return skillType.GetCustomAttributes(typeof(SkillDependencyAttribute), true)
                .Cast<SkillDependencyAttribute>()
                .Select(dependency => dependency.Target);
        }

        public static IEnumerable<Type> GetDependencies(Skill skill)
        {
            Argument.EnsureNotNull(skill, "skill");
            return GetDependencies(skill.GetType());
        }
        #endregion
        #endregion
    }
}
