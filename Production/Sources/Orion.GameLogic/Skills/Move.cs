using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        #endregion

        #region Constructors
        public Move(int speed)
        {
            Argument.EnsureStrictlyPositive(speed, "speed");
            this.speed = speed;
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
        #endregion
    }
}
