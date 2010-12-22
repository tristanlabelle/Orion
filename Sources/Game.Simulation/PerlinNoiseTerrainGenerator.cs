using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;

namespace Orion.Game.Simulation
{
    public class PerlinNoiseTerrainGenerator : TerrainGenerator
    {
        #region Fields
        private readonly Random random;
        private readonly Size size;
        #endregion

        #region Constructors
        public PerlinNoiseTerrainGenerator(Random random, Size size)
        {
            this.random = random;
            this.size = size;
        }
        #endregion

        #region Methods
        public override Terrain Generate()
        {
            PerlinNoise noise = new PerlinNoise(random);

            BitArray2D tiles = new BitArray2D(size);
            double[] rawTerrain = new double[size.Area];
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    rawTerrain[y * size.Width + x] = noise[x, y];
                }
            }

            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                tiles[k % size.Width, k / size.Width] = noiseValue >= 0.5;
                k++;
            }

            return new Terrain(tiles);
        }
        #endregion
    }
}
