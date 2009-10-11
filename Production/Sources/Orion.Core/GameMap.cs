using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Core
{
    public sealed class GameMap
    {

        #region Fields
        private bool[][] map;
        #endregion

        #region Constructors

        public GameMap(int MapWitdh, int MapHeight)
        {
            this.map = new bool[MapWitdh][];
            for (int i = 0; i < MapWitdh; i++)
            {
                this.map[i] = new bool[MapHeight];
            }
        }

        #endregion

        #region Properties

        #endregion

        #region Indexers

        public bool[] this[int x]
        {
            get { return this[x]; }
            set { map[x] = value; }
        }

        public bool this[int x, int y]
        {
            get { return map[x][y]; }
            set { map[x][y] = value; }
        }

        #endregion

        #region Methods

        public bool IsWalkable(int x, int y)
        {
            return map[x][y];
        }

        #endregion
    }
}
