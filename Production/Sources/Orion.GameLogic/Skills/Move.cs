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
        private readonly bool canFly;
        #endregion

        #region Constructors
        public Move(int speed, bool canFly)
        {
            Argument.EnsureStrictlyPositive(speed, "speed");
            Argument.EnsureNotNull(canFly, "canFly");
            this.speed = speed;
            this.canFly = canFly;
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

        public bool CanFly
        {
            get { return canFly; }
        }
        #endregion

        #region Methods
        public override int? TryGetBaseStat(UnitStat stat)
        {
            if (stat == UnitStat.MovementSpeed) return speed;
            if (stat == UnitStat.CanFly)
                if (canFly) return 1;
                else return 0;
            return null;
        }
        #endregion
    }
}
