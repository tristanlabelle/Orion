using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Technologies;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to research technologies.
    /// </summary>
    public sealed class Researcher : Component
    {
        #region Fields
        private readonly ValidatingCollection<string> technologies
            = new ValidatingCollection<string>(new HashSet<string>(), str => str != null);
        #endregion

        #region Constructors
        public Researcher(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of technologies this <see cref="Entity"/> can research.
        /// </summary>
        [Mandatory]
        public ICollection<string> Technologies
        {
            get { return technologies; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tests if a given <see cref="Technology"/> can be researched by this <see cref="Entity"/>.
        /// </summary>
        /// <param name="technology">The <see cref="Technology"/> to be tested.</param>
        /// <returns>
        /// <c>True</c> if this <see cref="Entity"/> can research <paramref name="technology"/>, <c>false</c> if not.
        /// </returns>
        public bool Supports(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");

            return Technologies.Contains(technology.Name);
        }
        #endregion
    }
}
