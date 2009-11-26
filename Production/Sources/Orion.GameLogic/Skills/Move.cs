using System;

namespace Orion.GameLogic.Skills
{
    /// <summary>
    /// A <see cref="Skill"/> which permits a <see cref="UnitType"/> to move,
    /// follow and do other tasks involving movement.
    /// </summary>
    [Serializable]
    public sealed class Move : Skill
    {
        #region Fields
        private readonly int speed;
        private readonly bool isAirborne;
        #endregion

        #region Constructors
        public Move(int speed, bool isAirborne)
        {
            Argument.EnsureStrictlyPositive(speed, "speed");
            Argument.EnsureNotNull(isAirborne, "isAirborne");
            this.speed = speed;
            this.isAirborne = isAirborne;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the moving speed associated with this skill.
        /// </summary>
        public float Speed
        {
            get { return speed; }
        }

        /// <summary>
        /// Gets a value indicating if entities with this skill are airborne.
        /// </summary>
        public bool IsAirborne
        {
            get { return isAirborne; }
        }
        #endregion

        #region Methods
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.MovementSpeed) return speed;
            if (stat == UnitStat.CanFly)
                if (isAirborne) return 1;
                else return 0;
            return null;
        }
        #endregion
    }
}
