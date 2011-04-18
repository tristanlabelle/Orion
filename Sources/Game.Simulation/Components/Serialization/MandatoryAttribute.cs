using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components.Serialization
{
    /// <summary>
    /// Indicates that the element must necessarily be present in the serialized form
    /// of the component.
    /// </summary>
    internal sealed class MandatoryAttribute : PersistentAttribute { }
}
