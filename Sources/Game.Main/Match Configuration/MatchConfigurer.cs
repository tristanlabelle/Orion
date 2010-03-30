using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Simulation;
using Orion.Game.Presentation;

namespace Orion.Game.Main
{
    abstract class MatchConfigurer
    {
        #region Fields
        protected MatchSettings settings = new MatchSettings();
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

        public MatchSettings Options
        {
            get { return settings; }
        }

        public MatchConfigurationUI UserInterface
        {
            get { return AbstractUserInterface; }
        }

        protected abstract MatchConfigurationUI AbstractUserInterface { get; }
        #endregion

        #region Methods
        public abstract void Start(out Match match, out SlaveCommander commander);

        protected void StartGame()
        {
            Action<MatchConfigurer> handler = GameStarted;
            if (handler != null) handler(this);
        }

        protected void CreateWorld(Size worldSize)
        {
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(settings.RandomSeed));
            random = new MersenneTwister(settings.RandomSeed);
            Terrain terrain = Terrain.Generate(worldSize, random);
            world = new World(terrain, random, settings.FoodLimit);
        }

        protected void TryPushCheatCodeExecutor(CommandPipeline pipeline, Match match)
        {
            if (settings.AreCheatsEnabled) pipeline.PushFilter(new CheatCodeExecutor(CheatCodeManager.Default, match));
        }

        protected void TryPushReplayRecorder(CommandPipeline pipeline)
        {
            Argument.EnsureNotNull(pipeline, "pipeline");

            ReplayRecorder replayRecorder = TryCreateReplayRecorder();
            if (replayRecorder != null) pipeline.PushFilter(replayRecorder);
        }

        private ReplayRecorder TryCreateReplayRecorder()
        {
            ReplayWriter replayWriter = ReplayWriter.TryCreate();
            if (replayWriter == null) return null;

            replayWriter.AutoFlush = true;
            replayWriter.WriteHeader(settings, world.Factions.Select(faction => faction.Name));

            return new ReplayRecorder(replayWriter);
        }
        #endregion
    }
}
