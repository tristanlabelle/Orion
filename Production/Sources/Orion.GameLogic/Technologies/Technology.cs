using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Orion.GameLogic.Technologies
{
    /// <summary>
    /// Represents a technology that, when researched, has an effect
    /// on one or more stats of a <see cref="UnitType"/>.
    /// </summary>
    /// <remarks>
    /// Possible requirements:
    /// - Resources
    /// - Other technologies
    /// Possible effects:
    /// - Alter a stat of unit types with a tag
    /// - Unlock a unit type
    /// </remarks>
    [Serializable]
    public sealed class Technology
    {
        #region Fields
        private readonly Handle handle;
        private readonly string name;
        private readonly TechnologyRequirements requirements;
        private readonly ReadOnlyCollection<TechnologyEffect> effects;
        #endregion

        #region Constructors
        public Technology(Handle handle, string name,
            TechnologyRequirements requirements, IEnumerable<TechnologyEffect> effects)
        {
            Argument.EnsureNotNullNorBlank(name, "name");
            Argument.EnsureNotNull(requirements, "requirements");
            Argument.EnsureNotNull(effects, "effects");

            this.handle = handle;
            this.name = name;
            this.requirements = requirements;
            this.effects = effects.ToList().AsReadOnly();
            Argument.EnsureStrictlyPositive(this.effects.Count, "effects.Count");
        }

        public Technology(Handle handle, string name,
            TechnologyRequirements requirements, params TechnologyEffect[] effects)
            : this(handle, name, requirements, (IEnumerable<TechnologyEffect>)effects)
        { }
        #endregion

        #region Properties
        public Handle Handle
        {
            get { return handle; }
        }

        /// <summary>
        /// Gets the name of this <see cref="Technology"/>.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the <see cref="TechnologyRequirements"/> which describes the preconditions
        /// needed to research this technology.
        /// </summary>
        public TechnologyRequirements Requirements
        {
            get { return requirements; }
        }

        public int AladdiumCost
        {
            get { return requirements.AladdiumCost; }
        }

        public int AlageneCost
        {
            get { return requirements.AlageneCost; }
        }

        public IEnumerable<Technology> RequiredTechnologies
        {
            get { return requirements.Technologies; }
        }

        /// <summary>
        /// Gets the sequence of this <see cref="Technology"/>'s effects.
        /// </summary>
        public IEnumerable<TechnologyEffect> Effects
        {
            get { return effects; }
        }
        #endregion

        #region Methods
        public int GetEffect(UnitType unitType, UnitStat stat)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return effects.Where(effect => effect.AppliesTo(unitType) && effect.Stat == stat)
                .Sum(effect => effect.Value);
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
