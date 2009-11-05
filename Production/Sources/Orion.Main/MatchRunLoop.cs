using System.Diagnostics;

using Orion.GameLogic;
using Orion.UserInterface;

namespace Orion.Main
{
    class MatchRunLoop : RunLoop
    {
        public const float targetFramesPerSecond = 60;
        public const float targetSecondsPerFrame = 1.0f / targetFramesPerSecond;

        private Stopwatch stopwatch;
        private Match match;
        private World world;

        public MatchRunLoop(GameUI ui, World world, Match match)
            : base(ui)
        {
            Argument.EnsureNotNull(ui, "ui");
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(match, "match");

            stopwatch = Stopwatch.StartNew();
            this.world = world;
            this.match = match;
        }

        protected override void RunLoopMain()
        {
            float timeDeltaInSeconds = (float)stopwatch.Elapsed.TotalSeconds;
            if (timeDeltaInSeconds >= targetSecondsPerFrame)
            {
                stopwatch.Stop();
                stopwatch.Reset();
                stopwatch.Start();

                do
                {
                    match.Pipeline.Update(targetFramesPerSecond);
                    world.Update(targetSecondsPerFrame);
                    userInterface.Update(targetSecondsPerFrame);

                    timeDeltaInSeconds -= targetSecondsPerFrame;
                } while (timeDeltaInSeconds >= targetSecondsPerFrame);
            }
        }

    }
}
