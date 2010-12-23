using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using OpenTK;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Generates random contents in a world.
    /// </summary>
    public abstract class WorldGenerator
    {
        #region Methods
        public abstract Terrain GenerateTerrain();
        public abstract void PrepareWorld(World world, UnitTypeRegistry unitTypes);
        #endregion
    }
}
