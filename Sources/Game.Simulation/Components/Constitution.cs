using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Describes the constitution of an <see cref="Entity"/>, which determines if
    /// it supports being repaired or healed.
    /// </summary>
    [Serializable]
    public enum Constitution
    {
        /// <summary>
        /// Indicates that the <see cref="Entity"/> is biological, and therefore supports being healed.
        /// </summary>
        Biological,

        /// <summary>
        /// Indicates that the <see cref="Entity"/> is mechanical, and therefore supports being repaired.
        /// </summary>
        Mechanical
    }
}
