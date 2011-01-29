using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components.Serialization
{
    /// <summary>
    /// Indicates that an attribute is permanent to a component.
    /// </summary>
    /// <remarks>
    /// Properties with this attribute are always serialized when their owning component is. Therefore,
    /// these should indicate the minimum set of data a component needs to be usable. Transient properties,
    /// properties whose represented value change during the course of a game, should not have this attribute
    /// set.
    /// </remarks>
    class PersistentAttribute : Attribute { }
}
