using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> as being a drop point for harvested resources.
    /// </summary>
    public sealed class ResourceDepot : Component
    {
        #region Constructors
        public ResourceDepot(Entity entity) : base(entity) { }
        #endregion
    }
}
