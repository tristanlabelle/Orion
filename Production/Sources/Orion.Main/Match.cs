using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Networking;

namespace Orion.Main
{
    class Match
    {
        #region Fields
        private static DateTime unixEpochStart = new DateTime(1970, 1, 1);

        private List<Faction> factions = new List<Faction>();

        protected Random random;
        protected CommandPipeline pipeline;
        protected Terrain terrain;
        protected World world;
        #endregion

        #region Constructors
        public Match(Random randomGenerator, Terrain terrain, World world)
        {
            random = randomGenerator;
            this.terrain = terrain;
            this.world = world;
        }
        #endregion

        #region Properties
        public IEnumerable<Faction> Factions
        {
            get { return factions; }
        }
        #endregion

        //public abstract void Start();
    }
}
