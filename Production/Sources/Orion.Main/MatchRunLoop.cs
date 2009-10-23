using System;
using System.Diagnostics;
using System.Windows.Forms;

using Orion.GameLogic;
using Orion.Graphics;
using Orion.Commandment;

namespace Orion.Main
{
    class MatchRunLoop : RunLoop
    {
        public const float targetFramesPerSecond = 30;
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
