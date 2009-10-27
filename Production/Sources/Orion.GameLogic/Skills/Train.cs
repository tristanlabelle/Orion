using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to create new units.
    /// </summary>
    [Serializable]
    public sealed class Train
    {
        #region Fields
        private readonly Func<UnitType, bool> predicate;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Train"/> from a predicate matching the <see cref="UnitType"/>s
        /// that can be trained.
        /// </summary>
        /// <param name="predicate">A predicate that matches <see cref="UnitType"/>s that can be built.</param>
        public Train(Func<UnitType, bool> predicate)
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
