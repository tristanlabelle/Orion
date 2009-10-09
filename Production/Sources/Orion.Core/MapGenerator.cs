using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
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

        public static GameMap GenerateNewMap(int MapWitdh, int MapHeight, Random random)
        {
            GameMap map = new GameMap(MapWitdh, MapHeight);
            noise = new PerlinNoise(random);
            for(int i = 0; i < MapWitdh; i ++)
            {
                for (int j = 0; j < MapHeight; j ++)
                {
                    if (noise[i, j] < 0.5)
                        map[i, j] = true;
                    else
                        map[i, j] = false;
                }
            }
            return map;
        }

        #endregion
    }
}