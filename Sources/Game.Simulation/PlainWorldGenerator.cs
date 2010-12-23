using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simluation;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation
{
    public class PlainWorldGenerator : WorldGenerator
    {
        public override Terrain GenerateTerrain(Size size)
        {
            return new Terrain(new BitArray2D(size, false));
        }

        public override void PrepareWorld(World world, UnitTypeRegistry unitTypes)
        {
            // do nothing
        }
    }
}
