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

        public static GameMap GetNewMap(int MapHeight, int MapWitdh, Random random)
        {
            GameMap map = new GameMap(MapHeight, MapWitdh);
            noise = new PerlinNoise(random);
            for(double i = 0; i < MapWitdh; i += 0.5)
            {
                for (double j = 0; j < MapHeight; j += 0.5)
                {
                    /*if (noise[i, j] > 0.5)
                        map[i][j] = true;
                    else
                        map[i][j] = false;*/
                }
            }
            return map;
        }

        #endregion
    }
}