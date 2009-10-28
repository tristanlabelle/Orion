using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to erect buildings.
    /// </summary>
    [Serializable]
    [SkillDependency(typeof(Move))]
    public sealed class Build : Skill
    {
        #region Fields
        private readonly Func<UnitType, bool> predicate;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Build"/> from a predicate matching the <see cref="UnitType"/>s
        /// that can be built.
        /// </summary>
        /// <param name="predicate">A predicate that matches <see cref="UnitType"/>s that can be built.</param>
        public Build(Func<UnitType, bool> predicate)
        {
            Argument.EnsureNotNull(predicate, "predicate");
            this.predicate = predicate;
        }
        #endregion

        #region Methods
        public bool Supports(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return predicate(unitType);
        }
        #endregion
    }
}
