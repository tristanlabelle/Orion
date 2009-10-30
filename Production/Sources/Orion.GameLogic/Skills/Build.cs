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
    public sealed class Build : Skill
    {
        #region Fields
        private readonly Func<UnitType, bool> predicate;
        private readonly int speed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Build"/> from a predicate matching the <see cref="UnitType"/>s
        /// that can be built.
        /// </summary>
        /// <param name="predicate">A predicate that matches <see cref="UnitType"/>s that can be built.</param>
        public Build(Func<UnitType, bool> predicate, int speed)
        {
            Argument.EnsureNotNull(predicate, "predicate");
            Argument.EnsureStrictlyPositive(speed, "speed");
            this.predicate = predicate;
            this.speed = speed;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the building speed associated with this skill.
        /// </summary>
        public int Speed
        {
            get { return speed; }
        }
        #endregion

        #region Methods
        public bool Supports(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            if (!unitType.IsBuilding) return false;
            return predicate(unitType);
        }

        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.BuildingSpeed) return speed;
            return null;
        }
        #endregion
    }
}
