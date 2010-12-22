using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation
{
    public abstract class TerrainGenerator
    {
        #region Nested Types
        public class FullyWalkable : TerrainGenerator
        {
            #region Fields
            private readonly Size size;
            #endregion

            #region Constructors
            public FullyWalkable(Size size)
            {
                this.size = size;
            }
            #endregion

            public override Terrain Generate()
            {
                return new Terrain(new BitArray2D(size, false));
            }
        }
        #endregion

        /// <summary>
        /// Generates the Terrain with subclass-specific logic.
        /// </summary>
        /// <returns>A new Terrain object.</returns>
        public abstract Terrain Generate();
    }
}
