using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Orion.UserInterface;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        #region Fields
        protected static Color[] playerColors = new Color[]
        {
            Color.Red, Color.Cyan, Color.Yellow, Color.Orange,
            Color.Green, Color.Pink, Color.Tan, Color.Brown
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
            get { return 0; }
        }

        public int Seed
        {
            get { return seed; }
            protected set { seed = value; }
        }

        public UIDisplay UserInterface
        {
            get { return AbstractUserInterface; }
        }

        protected abstract UIDisplay AbstractUserInterface { get; }
        #endregion

        #region Methods
        protected void StartGame()
        {
            GenericEventHandler<MatchConfigurer> handler = GameStarted;
            if (handler != null) handler(this);
        }

        protected void CreateWorld()
        {
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(seed));
            random = new MersenneTwister(seed);
            Terrain terrain = Terrain.Generate(new Size(128, 128), random);
            world = new World(terrain);
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
