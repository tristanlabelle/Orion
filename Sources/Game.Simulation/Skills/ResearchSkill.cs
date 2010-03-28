using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation.Technologies;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="UnitType"/> to research
    /// new <see cref="Technology">technologies</see>.
    /// </summary>
    [Serializable]
    public sealed class ResearchSkill : UnitSkill
    {
        #region Fields
        private static readonly Func<string, bool> itemValidator = item => item != null;

        private ICollection<string> targets
            = new ValidatingCollection<string>(new HashSet<string>(), itemValidator);
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the collection of the names of research targets.
        /// </summary>
        public ICollection<string> Targets
        {
            get { return targets; }
        }
        #endregion

        #region Methods
        protected override void DoFreeze()
        {
            targets = new ReadOnlyCollection<string>(targets.ToList());
        }

        protected override UnitSkill Clone()
        {
            return new ResearchSkill
            {
                targets = new ValidatingCollection<string>(new HashSet<string>(targets), itemValidator)
            };
        }

        public bool Supports(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return targets.Contains(technology.Name);
        }
        #endregion
    }
}
