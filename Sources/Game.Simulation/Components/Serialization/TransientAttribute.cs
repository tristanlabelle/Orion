using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components.Serialization
{
    /// <summary>
    /// Indicates that a property must never be taken into account during the serialization process.
    /// </summary>
    class TransientAttribute : Attribute
    {
    }
}
