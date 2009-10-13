using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Core
{
    /// <summary>
    /// Defines a terrain generator which generates random terrains.
    /// </summary>
    public static class TerrainGenerator
    {
        #region Fields

        private static PerlinNoise noise;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="Terrain"/> to be generated using a <see cref="PerlinNoise"/>. 
        /// </summary>
        /// <param name="terrainWidth"> The width of the terrain to be generated.</param>
        /// <param name="terrainHeight"> The height of the terrain to be generated.</param>
        /// <param name="random"> The <see cref="MersenneTwister"/> to be used to generate the terrain.</param>
        /// <returns>A newly generated <see cref="Terrain"/>.</returns>
        public static Terrain GenerateNewTerrain(int terrainWidth, int terrainHeight, MersenneTwister random)
        {
            if (noise == null)
                noise = new PerlinNoise(random);

            Terrain terrain = new Terrain(terrainWidth, terrainHeight);
            double[] rawTerrain = new double[terrainWidth * terrainHeight];
            for(int i = 0; i < terrainHeight; i ++)
            {
                for (int j = 0; j < terrainWidth; j ++)
                {
                    rawTerrain[i * terrainWidth + j] = noise[j, i];
                }
            }
            
            double max = rawTerrain.Max();
            int k = 0;
            foreach (double noiseValue in rawTerrain.Select(d => d / max))
            {
                terrain[k % terrainHeight, k / terrainHeight] = noiseValue >= 0.5;
                k++;
            }

            return terrain;
        }

        #endregion
    }
}