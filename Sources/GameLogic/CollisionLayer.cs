using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Specifies the layer of collision in which an <see cref="Entity"/> is.
    /// <see cref="Entity">Entities</see> in different collision layers will not collide together.
    /// </summary>
    [Serializable]
    public enum CollisionLayer
    {
        /// <summary>
        /// Specifies that an <see cref="Entity"/> never collides with another one.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that an <see cref="Entity"/> is on the ground and may collide
        /// with other ground <see cref="Entity">entities</see>.
        /// </summary>
        Ground,

        /// <summary>
        /// Specifies that an <see cref="Entity"/> is in the air and may collide
        /// with other airborne <see cref="Entity">entities</see>.
        /// </summary>
        Air
    }
}
