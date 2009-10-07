using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    public sealed class MapGenerator
    {
        #region Fields
        
        double MapLength;
        double MapHeight;
        PerlinNoise noise;

        #endregion

        #region Constructors

        public MapGenerator(double MapLength, double MapHeight, Random random)
        {
            this.MapHeight = MapHeight;
            this.MapLength = MapLength;
            this.noise = new PerlinNoise(random);
        }
        
        #endregion

        #region Properties

        /*public bool Walkable
        {
            get {  }
        }*/

        #endregion

        #region Methods

        public void Generate()
        {
        
        }

        #endregion
    }
}