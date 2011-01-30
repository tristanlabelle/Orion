using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Indicates how units and buildings are trained.
    /// </summary>
    public enum TrainType
    {
        /// <summary>
        /// Indicates that the unit appears only once its training time is over.
        /// </summary>
        Immaterial,

        /// <summary>
        /// Indicates that the unit appears immediately, and then proceeds into being built.
        /// </summary>
        OnSite
    }
}
