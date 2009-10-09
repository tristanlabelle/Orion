using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Core
{
    public static class MapGenerator
    {
        #region Fields

        public static PerlinNoise noise;

        #endregion

        #region Constructors
        
        #endregion

        #region Properties

        #endregion

        #region Methods

        public static GameMap GenerateNewMap(int MapWidth, int MapHeight, MersenneTwister random)
        {
            if (noise == null)
                noise = new PerlinNoise(random);

            GameMap map = new GameMap(MapWidth, MapHeight);
            double[] rawMap = new double[MapWidth * MapHeight];
            for(int i = 0; i < MapHeight; i ++)
            {
                for (int j = 0; j < MapWidth; j ++)
                {
                    rawMap[i * MapWidth + j] = noise[j, i];
                }
            }
            
            double max = rawMap.Max();
            int k = 0;
            foreach (double noiseValue in rawMap.Select(d => d / max))
            {
                map[k % MapHeight, k / MapHeight] = noiseValue >= 0.5;
                k++;
            }

            return map;
        }

        #endregion
    }
}