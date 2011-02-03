using System;
using Orion.Engine;

namespace Orion.Game.Simulation.Skills
{
    /// <summary>
    /// A <see cref="UnitSkill"/> which permits a <see cref="Unit"/> to move,
    /// follow and do other tasks involving movement.
    /// </summary>
    [Serializable]
    public sealed class MoveSkill : UnitSkill
    {
        #region Fields
        public static readonly UnitStat SpeedStat = new UnitStat(typeof(MoveSkill), "Speed", "Vitesse de déplacement");

        private int speed = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the speed at which units move, in world units per second.
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set
            {
                EnsureNotFrozen();
                Argument.EnsureStrictlyPositive(value, "Speed");
                speed = value;
            }
        }
        #endregion

        #region Methods
        protected override UnitSkill Clone()
        {
            return new MoveSkill { speed = speed };
        }

        public override int GetStat(UnitStat stat)
        {
            if (stat == SpeedStat) return speed;
            return base.GetStat(stat);
        }

        protected override void DoSetStat(UnitStat stat, int value)
        {
            if (stat == SpeedStat) Speed = value;
            else base.DoSetStat(stat, value);
        }
        #endregion
    }
}
