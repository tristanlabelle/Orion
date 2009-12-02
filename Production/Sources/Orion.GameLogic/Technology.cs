using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Orion.GameLogic
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
        private readonly string name;
        private readonly Handle handle;
        private readonly TechnologyRequirements requirements;
        private readonly ReadOnlyCollection<TechnologyEffect> effects;
        #endregion

        #region Constructors
        public Technology(string name, TechnologyRequirements requirements, IEnumerable<TechnologyEffect> effects, Handle handle)
        {
            Argument.EnsureNotNullNorBlank(name, "name");
            Argument.EnsureNotNull(requirements, "requirements");
            Argument.EnsureNotNull(effects, "effects");

            this.name = name;
            this.requirements = requirements;
            this.effects = effects.ToList().AsReadOnly();
            Argument.EnsureStrictlyPositive(this.effects.Count, "effects.Count");
            this.handle = handle;
        }
        #endregion

        #region Properties
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

        /// <summary>
        /// Gets the sequence of this <see cref="Technology"/>'s effects.
        /// </summary>
        public IEnumerable<TechnologyEffect> Effects
        {
            get { return effects; }
        }

        public Handle Handle
        {
            get { return handle; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
