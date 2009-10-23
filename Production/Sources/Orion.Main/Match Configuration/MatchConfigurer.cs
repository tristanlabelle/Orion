using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;
using Orion.GameLogic;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        protected int seed;
        protected World world;
        protected Random random;
        protected Terrain terrain;

        protected void CreateMap()
        {
            random = new MersenneTwister(seed);
            terrain = Terrain.Generate(128, 128, random);
            world = new World(terrain);
            Console.WriteLine("Mersenne twister seed: {0}", seed);
        }

		public abstract Match Start();
    }
}
