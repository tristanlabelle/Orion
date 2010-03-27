using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// Abstract base class for skills, which denotes the actions that can be done by units.
    /// </summary>
    [Serializable]
    public abstract class UnitSkill
    {
        #region Instance
        #region Fields
        private bool isFrozen;
        #endregion

        #region Constructors
        internal UnitSkill() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this instance is "frozen".
        /// That is, if no more modifications can be made to it.
        /// </summary>
        public bool IsFrozen
        {
            get { return isFrozen; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves the value of a <see cref="UnitStat"/> exposed by this <see cref="UnitSkill"/>.
        /// </summary>
        /// <param name="stat">The <see cref="UnitStat"/> which's value is to be retrieved.</param>
        /// <returns>The value associated to this <see cref="UnitStat"/>.</returns>
        public virtual int GetStat(UnitStat stat)
        {
            throw new InvalidSkillStatException(GetType(), stat);
        }

        /// <summary>
        /// Modifies the value of a stat of this skill.
        /// </summary>
        /// <param name="stat">The stat to be modified.</param>
        /// <param name="value">The new value of that stat.</param>
        public void SetStat(UnitStat stat, int value)
        {
            EnsureNotFrozen();
            Argument.EnsureNotNull(stat, "stat");
            Argument.EnsurePositive(value, "value");

            DoSetStat(stat, value);
        }

        protected virtual void DoSetStat(UnitStat stat, int value)
        {
            throw new InvalidSkillStatException(GetType(), stat);
        }

        /// <summary>
        /// Marks this skill as "frozen", preventing further modifications.
        /// </summary>
        private void Freeze()
        {
            if (isFrozen) return;
            isFrozen = true;
            DoFreeze();
        }

        protected virtual void DoFreeze() { }

        /// <summary>
        /// Creates a new clone of this skill, with the same values.
        /// </summary>
        /// <returns>A new clone of this skill.</returns>
        protected abstract UnitSkill Clone();

        /// <summary>
        /// Creates a clone of this skill which is frozen.
        /// </summary>
        /// <returns>A newly created frozen clone.</returns>
        public UnitSkill CreateFrozenClone()
        {
            UnitSkill clone = Clone();
            clone.Freeze();
            return clone;
        }

        protected void EnsureNotFrozen()
        {
            if (isFrozen) throw new InvalidOperationException("Cannot modify a frozen skill.");
        }
        #endregion
        #endregion

        #region Static
        #region Nested Types
        /// <summary>
        /// Holds the collections of all skill types and stats.
        /// </summary>
        /// <remarks>
        /// This class exists to solve an initialization order problem
        /// which appears if the <see cref="UnitSkill"/> class attempts to
        /// reflect members of its decendent classes in its static constructor.
        /// </remarks>
        private static class Repository
        {
            #region Fields
            public static readonly Dictionary<string, Type> Types;
            public static readonly ReadOnlyCollection<UnitStat> Stats;
            #endregion

            #region Constructor
            static Repository()
            {
                Type baseSkillType = typeof(UnitSkill);
                Types = Assembly.GetExecutingAssembly()
                    .GetExportedTypes()
                    .Where(type => typeof(UnitSkill).IsAssignableFrom(type)
                        && type.Name.EndsWith("Skill")
                        && type != baseSkillType
                        && !type.IsAbstract)
                    .ToDictionary(type => GetTypeName(type));

                Stats = Types.Values
                    .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public))
                    .Where(field => field.FieldType == typeof(UnitStat))
                    .Select(field => {
                        UnitStat stat = (UnitStat)field.GetValue(null);
                        Debug.Assert(stat != null);
                        return stat;
                    })
                    .ToList()
                    .AsReadOnly();
            }
            #endregion
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of concrete <see cref="UnitSkill"/> types which are defined.
        /// </summary>
        public static IEnumerable<Type> Types
        {
            get { return Repository.Types.Values; }
        }

        /// <summary>
        /// Gets the collection of <see cref="UnitStat"/> which are defined.
        /// </summary>
        public static ReadOnlyCollection<UnitStat> Stats
        {
            get { return Repository.Stats; }
        }
        #endregion

        #region Methods
        #region Names
        /// <summary>
        /// Gets the name of a skill type.
        /// </summary>
        /// <param name="type">The skill type.</param>
        /// <returns>The name of that skill type.</returns>
        public static string GetTypeName(Type type)
        {
            Argument.EnsureNotNull(type, "type");

            return type.Name.Substring(0, type.Name.Length - "Skill".Length);
        }

        /// <summary>
        /// Gets the name of a skill type.
        /// </summary>
        /// <param name="skill">The skill which's type name is to be found.</param>
        /// <returns>The name of that skill type.</returns>
        public static string GetTypeName(UnitSkill skill)
        {
            Argument.EnsureNotNull(skill, "skill");
            return GetTypeName(skill.GetType());
        }

        /// <summary>
        /// Attempts to retreive a skill type from its name.
        /// </summary>
        /// <param name="name">The name of the skill.</param>
        /// <returns>The corresponding skill type, if any.</returns>
        public static Type GetTypeFromName(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Type type;
            Repository.Types.TryGetValue(name, out type);
            return type;
        }
        #endregion

        #region Stats
        public static UnitStat GetStat(Type skillType, string statName)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            Argument.EnsureNotNull(statName, "statName");

            string fullName = GetTypeName(skillType) + '.' + statName;
            return GetStat(fullName);
        }

        public static UnitStat GetStat(UnitSkill skill, string statName)
        {
            Argument.EnsureNotNull(skill, "skill");

            return GetStat(skill.GetType(), statName);
        }

        public static UnitStat GetStat(string fullName)
        {
            Argument.EnsureNotNull(fullName, "fullName");

            if (fullName.IndexOf('.') == -1)
                fullName = GetTypeName(typeof(BasicSkill)) + '.' + fullName;

            return Repository.Stats.FirstOrDefault(stat => stat.FullName == fullName);
        }
        #endregion

        #region Dependencies
        public static IEnumerable<Type> GetDependencies(Type skillType)
        {
            Argument.EnsureNotNull(skillType, "skillType");
            return skillType.GetCustomAttributes(typeof(SkillDependencyAttribute), true)
                .Cast<SkillDependencyAttribute>()
                .Select(dependency => dependency.Target);
        }

        public static IEnumerable<Type> GetDependencies(UnitSkill skill)
        {
            Argument.EnsureNotNull(skill, "skill");
            return GetDependencies(skill.GetType());
        }
        #endregion
        #endregion
        #endregion
    }
}
