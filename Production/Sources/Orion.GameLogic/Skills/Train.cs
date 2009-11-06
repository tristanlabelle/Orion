using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to create new units.
    /// </summary>
    [Serializable]
    public sealed class Train : Skill
    {
        #region Fields
        private readonly Func<UnitType, bool> predicate;
        private readonly int speed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Train"/> from a predicate matching the <see cref="UnitType"/>s
        /// that can be trained.
        /// </summary>
        /// <param name="predicate">A predicate that matches <see cref="UnitType"/>s that can be built.</param>
        /// <param name="speed">The training speed, in health points per second.</param>
        public Train(Func<UnitType, bool> predicate, int speed)
        {
            Argument.EnsureNotNull(predicate, "predicate");
            Argument.EnsureStrictlyPositive(speed, "speed");
            this.predicate = predicate;
            this.speed = speed;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the speed at which units are trained, in health points per second.
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
            if (unitType.IsBuilding) return false;
            return predicate(unitType);
        }

        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.TrainingSpeed) return speed;
            return null;
        }
        #endregion
    }
}
