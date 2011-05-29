using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> as being able to extract alagene nodes underneath it.
    /// </summary>
    public sealed class AlageneExtractor : Component
    {
        public AlageneExtractor(Entity entity) : base(entity) { }

        public override int GetStateHashCode()
        {
            return 0;
        }
    }
}
