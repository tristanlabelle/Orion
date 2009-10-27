using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to research
    /// new <see cref="Technology">technologies</see>.
    /// </summary>
    [Serializable]
    public sealed class Research
    {
        #region Fields
        private readonly Func<Technology, bool> predicate;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Research"/> from a predicate matching
        /// the <see cref="Technology">technologies</see> that can be researched.
        /// </summary>
        /// <param name="predicate">
        /// A predicate that matches the <see cref="Technology">technologies</see> that can be researched.
        /// </param>
        public Research(Func<Technology, bool> predicate)
        {
            Argument.EnsureNotNull(predicate, "predicate");
            this.predicate = predicate;
        }
        #endregion

        #region Methods
        public bool Supports(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            return predicate(technology);
        }
        #endregion
    }
}
