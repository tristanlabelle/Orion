using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Matchmaking;
using Orion.Matchmaking.Commands.Pipeline;
using Orion.GameLogic;
using Orion.UserInterface;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        #region Fields
        private int seed;
        protected World world;
        protected Random random;
        protected int numberOfPlayers = 2;
        #endregion

        #region Events

        public event Action<MatchConfigurer> GameStarted;

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
            Action<MatchConfigurer> handler = GameStarted;
            if (handler != null) handler(this);
        }

        protected void CreateWorld(Size worldSize)
        {
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(seed));
            random = new MersenneTwister(seed);
            Terrain terrain = Terrain.Generate(worldSize, random);
            world = new World(terrain, random);
        }

        protected void TryPushReplayRecorderToPipeline(CommandPipeline pipeline)
        {
            Argument.EnsureNotNull(pipeline, "pipeline");

            ReplayRecorder replayRecorder = TryCreateReplayRecorder();
            if (replayRecorder != null) pipeline.PushFilter(replayRecorder);
        }

        protected ReplayRecorder TryCreateReplayRecorder()
        {
            ReplayWriter replayWriter = ReplayWriter.TryCreate();
            if (replayWriter == null) return null;

            replayWriter.AutoFlush = true;
            replayWriter.WriteHeader(Seed, world.Factions.Select(faction => faction.Name));

            return new ReplayRecorder(replayWriter);
        }

        public abstract void Start(out Match match, out SlaveCommander commander);
        #endregion
    }
}