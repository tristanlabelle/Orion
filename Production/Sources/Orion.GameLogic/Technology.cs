using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents a technology that, when researched, has an effect
    /// on one or more stats of a <see cref="UnitType"/>.
    /// </summary>
    [Serializable]
    public sealed class Technology
    {
        #region Fields
        private readonly string name;
        private readonly ReadOnlyCollection<TechnologyEffect> effects;
        #endregion

        #region Constructors
        public Technology(string name, IEnumerable<TechnologyEffect> effects)
        {
            Argument.EnsureNotNullNorBlank(name, "name");
            Argument.EnsureNotNull(effects, "effects");

            this.name = name;
            this.effects = effects.ToList().AsReadOnly();
            Argument.EnsureStrictlyPositive(this.effects.Count, "effects.Count");
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
        /// Gets the read-only list of this <see cref="Technology"/>'s effects.
        /// </summary>
        public IList<TechnologyEffect> Effects
        {
            get { return effects; }
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
