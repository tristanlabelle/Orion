using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.GameLogic
{
    public struct HitEventArgs
    {
        #region Fields
        public readonly Unit Hitter;
        public readonly Unit Target;
        public readonly float Damage;
        #endregion

        #region Constructors
        public HitEventArgs(Unit hitter, Unit target, float damage)
        {
            Argument.EnsureNotNull(hitter, "hitter");
            Argument.EnsureNotNull(target, "target");
            Argument.EnsureStrictlyPositive(damage, "damage");

            this.Hitter = hitter;
            this.Target = target;
            this.Damage = damage;
        }
        #endregion
    }
}
