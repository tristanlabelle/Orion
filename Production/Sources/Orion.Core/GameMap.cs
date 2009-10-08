using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    public sealed class GameMap
    {

        #region Fields
        private bool[][] map;
        #endregion

        #region Constructors

        public GameMap(int MapHeight, int MapWitdh)
        {
            
        }

        #endregion

        #region Properties

        #endregion

        #region Indexers

        public bool this[int x, int y]
        {
            get { return map[x][y]; }
            set { map[x][y] = value; }
        }

        #endregion

        #region Methods

        #endregion
    }
}
