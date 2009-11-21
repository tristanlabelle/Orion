using System;
using Orion.UserInterface;
using Orion.GameLogic;
using Orion.Commandment;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        #region Fields
        protected static Color[] playerColors = new Color[]
        {
            Color.Red, Color.Cyan, Color.Purple, Color.Yellow, Color.Orange,
            Color.Green, Color.Pink, Color.Tan, Color.Turquoise, Color.Brown, Color.LightGray
        };

        private int seed;
        protected World world;
        protected Random random;
        protected int numberOfPlayers = 2;
        #endregion

        #region Events

        public event GenericEventHandler<MatchConfigurer> GameStarted;

        #endregion

        #region Properties
        public int NumberOfPlayers
        {
            get { return UserInterface.NumberOfPlayers; }
        }

        public int Seed
        {
            get { return seed; }
            protected set { seed = value; }
        }

        public MatchConfigurationUI UserInterface
        {
            get { return AbstractUserInterface; }
        }

        protected abstract MatchConfigurationUI AbstractUserInterface { get; }
        #endregion

        #region Methods
        protected void StartGame()
        {
            GenericEventHandler<MatchConfigurer> handler = GameStarted;
            if (handler != null) handler(this);
        }

        protected void CreateMap()
        {
            Console.WriteLine("Mersenne Twister Seed: {0}", seed);
            random = new MersenneTwister(seed);
            Terrain terrain = Terrain.Generate(128, 128, random);
            world = new World(terrain);
        }

        public abstract Match Start();
        #endregion
    }
}
