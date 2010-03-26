using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    public class MatchOptions
    {
        #region Fields
        private Size mapSize = new Size(150, 150);
        private MatchStartType startType = MatchStartType.Sedentary;
        private int maximumPopulation = 200;
        private bool revealTopology = false;
        private int initialAladdiumAmount = 200;
        private int initialAlageneAmount = 0;
        private int seed = 0;
        #endregion

        #region Properties
        public Size MapSize
        {
            get { return mapSize; }
            set { mapSize = value; }
        }

        public MatchStartType StartType
        {
            get { return startType; }
            set { startType = value; }
        }

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public int MaximumPopulation
        {
            get { return maximumPopulation; }
            set { maximumPopulation = value; }
        }

        public bool RevealTopology
        {
            get { return revealTopology; }
            set { revealTopology = value; }
        }

        public int InitialAladdiumAmount
        {
            get { return initialAladdiumAmount; }
            set { initialAladdiumAmount = value; }
        }

        public int InitialAlageneAmount
        {
            get { return initialAlageneAmount; }
            set { initialAlageneAmount = value; }
        }
        #endregion
    }
}
