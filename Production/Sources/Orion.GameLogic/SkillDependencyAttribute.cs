using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic
{
    /// <summary>
    /// Marks a skill as being dependant on another skill.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public sealed class SkillDependencyAttribute : Attribute
    {
        #region Fields
        private readonly Type target;
        #endregion

        #region Constructors
        public SkillDependencyAttribute(Type target)
        {
            Argument.EnsureNotNull(target, "target");
            if (!(typeof(Skill).IsAssignableFrom(target)))
                throw new ArgumentException("Skill dependency target should be another skill.", "target");

            this.target = target;
        }
        #endregion

        #region Properties
        public Type Target
        {
            get { return target; }
        }
        #endregion
    }
}
