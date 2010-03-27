using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Simulation;
using Orion.Game.Presentation;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        #region Fields
        protected MatchOptions options = new MatchOptions();
        protected World world;
        protected Random random;
        #endregion

        #region Events

        public event Action<MatchConfigurer> GameStarted;

        #endregion

        #region Properties
        public int NumberOfPlayers
        {
            get { return UserInterface.NumberOfPlayers; }
        }

        public MatchOptions Options
        {
            get { return options; }
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
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(options.Seed));
            random = new MersenneTwister(options.Seed);
            Terrain terrain = Terrain.Generate(worldSize, random);
            world = new World(terrain, random, options.MaximumPopulation);
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
            replayWriter.WriteHeader(options, world.Factions.Select(faction => faction.Name));

            return new ReplayRecorder(replayWriter);
        }

        public abstract void Start(out Match match, out SlaveCommander commander);
        #endregion
    }
}
