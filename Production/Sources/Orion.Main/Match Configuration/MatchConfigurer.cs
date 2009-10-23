using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Graphics;
using Orion.GameLogic;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
		protected static Color[] playerColors = new Color[] { Color.Red, Color.Cyan, Color.Purple, Color.Yellow, Color.Orange,
			Color.Green, Color.Pink, Color.Tan, Color.Turquoise, Color.Brown };
		
		protected int numberOfPlayers = 2;
        protected int seed;
        protected World world;
        protected Random random;
        protected Terrain terrain;

        protected void CreateMap()
        {
            random = new MersenneTwister(seed);
            terrain = Terrain.Generate(128, 128, random);
            world = new World(terrain);
        }
		
		public int NumberOfPlayers
		{
			get { return numberOfPlayers; }
			set
			{
				Argument.EnsureWithin(value, 1, playerColors.Length, "value");
				numberOfPlayers = value;
			}
		}

        /// <summary>
        /// Sets the seed manually. (For debugging purposes.)
        /// </summary>
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

		public abstract Match Start();
    }
}