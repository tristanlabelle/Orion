using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Orion.Engine;
using Orion.Game.Simulation.Skills;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Provides a mutable representation of a <see cref="UnitType"/>
    /// as a way to create instances.
    /// </summary>
    [Serializable]
    public sealed class UnitTypeBuilder
    {
        #region Fields
        private string name;
        private Size size;
        private bool isAirborne;
        private BasicSkill basicSkill;
        private readonly ICollection<UnitSkill> skills;
        private string heroUnitType;
        #endregion

        #region Constructors
        public UnitTypeBuilder()
        {
            Func<UnitSkill, bool> predicate = item =>
                item != null
                && skills.None(skill => skill.GetType() == item.GetType())
                && item.GetType() != typeof(BasicSkill);
            skills = new ValidatingCollection<UnitSkill>(new HashSet<UnitSkill>(), predicate);

            Reset();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNullNorBlank(value, "Name");
                this.name = value;
            }
        }

        public Size Size
        {
            get { return size; }
            set
            {
                Argument.EnsureWithin(value.Width, 0, Entity.MaxSize, "Size.Width");
                Argument.EnsureWithin(value.Height, 0, Entity.MaxSize, "Size.Height");
                this.size = value;
            }
        }

        public int Width
        {
            get { return size.Width; }
            set { size = new Size(value, size.Height); }
        }

        public int Height
        {
            get { return size.Height; }
            set { size = new Size(size.Width, value); }
        }

        public bool IsAirborne
        {
            get { return isAirborne; }
            set { isAirborne = value; }
        }

        public BasicSkill BasicSkill
        {
            get { return basicSkill; }
        }

        public ICollection<UnitSkill> Skills
        {
            get { return skills; }
        }

        public string HeroUnitType
        {
            get { return heroUnitType; }
            set { heroUnitType = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets this builder to the default values.
        /// </summary>
        public void Reset()
        {
            name = null;
            size = new Size(1, 1);
            isAirborne = false;
            basicSkill = new BasicSkill();
            skills.Clear();
        }

        public void Validate()
        {
            if (name == null) throw new InvalidOperationException("UnitType has no name.");

            foreach (UnitSkill skill in skills)
            {
                Type firstMissingDependencyType = UnitSkill.GetDependencies(skill.GetType())
                    .FirstOrDefault(dependencyType => skills.None(s => s.GetType() == dependencyType));

                if (firstMissingDependencyType != null)
                {
                    throw new InvalidOperationException(
                        "Cannot build unit type, {0} skill dependency is missing."
                        .FormatInvariant(UnitSkill.GetTypeName(firstMissingDependencyType)));
                }
            }
        }

        public UnitType Build(Handle handle)
        {
            Validate();
            return new UnitType(handle, this);
        }
        #endregion
    }
}
