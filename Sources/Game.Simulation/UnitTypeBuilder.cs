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
        private string graphicsTemplate;
        private string voicesTemplateName;
        private Size size;
        private bool isAirborne;
        private BasicSkill basicSkill;
        private readonly ICollection<UnitSkill> skills;
        private readonly ICollection<UnitTypeUpgrade> upgrades;
        #endregion

        #region Constructors
        public UnitTypeBuilder()
        {
            skills = new ValidatingCollection<UnitSkill>(item =>
                item != null
                && skills.None(skill => skill.GetType() == item.GetType())
                && item.GetType() != typeof(BasicSkill));

            upgrades = new ValidatingCollection<UnitTypeUpgrade>(item =>
                item != null
                && upgrades.None(upgrade => upgrade.Target == item.Target));

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
                name = value;
            }
        }

        public string GraphicsTemplate
        {
            get { return graphicsTemplate; }
            set { graphicsTemplate = value; }
        }

        public string VoicesTemplate
        {
            get { return voicesTemplateName; }
            set { voicesTemplateName = value; }
        }

        public Size Size
        {
            get { return size; }
            set
            {
                Argument.EnsureWithin(value.Width, 0, Entity.MaxSize, "Size.Width");
                Argument.EnsureWithin(value.Height, 0, Entity.MaxSize, "Size.Height");
                size = value;
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

        public ICollection<UnitTypeUpgrade> Upgrades
        {
            get { return upgrades; }
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
