using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Orion.GameLogic
{
    /// <summary>
    /// Provides a strongly-typed collection of <see cref="Skill"/>s which
    /// disallows multiple skills of the same type.
    /// </summary>
    [Serializable]
    public sealed class SkillCollection : ICollection<Skill>
    {
        #region Fields
        private readonly List<Skill> skills = new List<Skill>(8);
        #endregion

        #region Properties
        public int Count
        {
            get { return skills.Count; }
        }
        #endregion

        #region Methods
        public void Add(Skill newSkill)
        {
            Argument.EnsureNotNull(newSkill, "newSkill");

            bool skillTypeExists = skills.Select(skill => skill.GetType())
                .Any(skillType => skillType == newSkill.GetType());
            if (skillTypeExists)
            {
                throw new InvalidOperationException(
                    "Cannot add a new \"{0}\" skill as one already exists."
                    .FormatInvariant(newSkill.GetType().FullName));
            }

            Type firstMissingDependency = Skill.GetDependencies(newSkill)
                .FirstOrDefault(dependency => skills.All(skill => skills.GetType() != dependency));
            if (firstMissingDependency != null)
            {
                throw new InvalidOperationException(
                    "Cannot add skill \"{0}\" when dependency \"{1}\" is not present."
                    .FormatInvariant(newSkill.GetType().FullName, firstMissingDependency.FullName));
            }

            skills.Add(newSkill);
        }

        public TSkill Find<TSkill>() where TSkill : Skill
        {
            return skills.OfType<TSkill>().FirstOrDefault();
        }

        public bool Contains(Skill skill)
        {
            return skills.Contains(skill);
        }

        public bool Remove(Skill skill)
        {
            int index = skills.IndexOf(skill);
            if (index == -1) return false;

            Type firstDependentSkillType = Enumerable.Range(0, skills.Count)
                .Where(i => i != index)
                .Select(i => skills[i].GetType())
                .FirstOrDefault(skillType => Skill.GetDependencies(skill).Contains(skillType));
            if (firstDependentSkillType != null)
            {
                throw new InvalidOperationException(
                    "Cannot remove skill \"{0}\" as \"{1}\" depends on it."
                    .FormatInvariant(skill.GetType().FullName, firstDependentSkillType.FullName));
            }

            skills.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            skills.Clear();
        }

        public List<Skill>.Enumerator GetEnumerator()
        {
            return skills.GetEnumerator();
        }
        #endregion

        #region Explicit Members
        #region ICollection<Skill> Members
        void ICollection<Skill>.CopyTo(Skill[] array, int arrayIndex)
        {
            skills.CopyTo(array, arrayIndex);
        }

        bool ICollection<Skill>.IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region IEnumerable<Skill> Members
        IEnumerator<Skill> IEnumerable<Skill>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
    }
}
