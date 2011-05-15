using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Allows an <see cref="Entity"/> to be assigned tasks which imply movement.
    /// </summary>
    public sealed class Mobile : Component
    {
        #region Fields
        public static readonly Stat SpeedStat = new Stat(typeof(Mobile), StatType.Real, "Speed");

        private float speed = 1;
        #endregion

        #region Constructors
        public Mobile(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the maximum movement speed of the host <see cref="Entity"/>.
        /// </summary>
        [Persistent(true)]
        public float Speed
        {
            get { return speed; }
            set
            {
                Argument.EnsureStrictlyPositive(value, "Speed");
                speed = value;
            }
        }
        #endregion
    }
}
