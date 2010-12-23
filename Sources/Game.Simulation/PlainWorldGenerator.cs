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
        #region Fields
        private readonly Size size;
        #endregion

        #region Constructors
        public PlainWorldGenerator(Size size)
        {
            this.size = size;
        }
        #endregion

        #region Methods
        public override Terrain GenerateTerrain()
        {
            return new Terrain(new BitArray2D(size, false));
        }

        public override void PrepareWorld(World world, UnitTypeRegistry unitTypes)
        {
            // do nothing
        }
        #endregion
    }
}
