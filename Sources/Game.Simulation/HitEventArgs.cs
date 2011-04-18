using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Describes an event raised when an <see cref="Entity"/> hits another one.
    /// </summary>
    public struct HitEventArgs
    {
        #region Fields
        private readonly Entity hitter;
        private readonly Entity target;
        #endregion

        #region Constructors
        public HitEventArgs(Entity hitter, Entity target)
        {
            Argument.EnsureNotNull(hitter, "hitter");
            Argument.EnsureNotNull(target, "target");

            this.hitter = hitter;
            this.target = target;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Entity"/> which inflicted the damage.
        /// </summary>
        public Entity Hitter
        {
            get { return hitter; }
        }

        /// <summary>
        /// Gets the <see cref="Entity"/> to which the damage was inflicted.
        /// </summary>
        public Entity Target
        {
            get { return target; }
        }
        #endregion
    }
}
