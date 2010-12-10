using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Engine.Gui2;
using Application = System.Windows.Forms.Application;

namespace Orion.Game.Main
{
    internal class Program
    {
        #region Fields
        private const float TargetFramesPerSecond = 40;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const float TimeSpeedMultiplier = 1;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;
        #endregion

        #region Methods
        #region Logging Utilities
        private static void EnableLogging()
        {
            foreach (string logFileName in GetPossibleLogFileNames())
            {
                try
                {
                    var stream = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    var writer = new StreamWriter(stream);
#if DEBUG
                    writer.AutoFlush = true;
#endif
                    Trace.Listeners.Add(new TextWriterTraceListener(writer));
                    break;
                }
                catch (IOException) { }
            }
        }

        private static IEnumerable<string> GetPossibleLogFileNames()
        {
            const string baseFileNameWithoutExtension = "Log";
            const string extension = ".txt";
            yield return baseFileNameWithoutExtension + extension;
            for (int i = 2; i < 10; ++i)
                yield return "{0} ({1}){2}".FormatInvariant(baseFileNameWithoutExtension, i, extension);
        }
        #endregion

        #region Main
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (GameGraphics gameGraphics = new GameGraphics())
            {
                UIManager uiManager = new UIManager(gameGraphics.Context);

                StackPanel stackPanel = new StackPanel
                {
                    HorizontalAlignment = Alignment.Center,
                    VerticalAlignment = Alignment.Center,
                    ItemGap = 10
                };

                stackPanel.Children.Add(new Label { Text = "Foo" });
                stackPanel.Children.Add(new Label { Text = "Bar" });
                stackPanel.Children.Add(new Button("Frob") { Padding = new Borders(0, 0, 20, 0) });
                uiManager.Root = stackPanel;

                while (true)
                {
                    gameGraphics.Window.Update();
                    gameGraphics.Context.Clear(Colors.Green);
                    gameGraphics.Context.ProjectionBounds = new Engine.Geometry.Rectangle(0, 0, uiManager.Size.Width, uiManager.Size.Height);
                    uiManager.Draw();
                    gameGraphics.Context.Present();
                }

                using (GameStateManager gameStateManager = new GameStateManager())
                {
                    gameStateManager.Push(new MainMenuGameState(gameStateManager, gameGraphics));

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    FrameRateCounter updateRateCounter = new FrameRateCounter();
                    FrameRateCounter drawRateCounter = new FrameRateCounter();

                    // This run loop uses a fixed time step for the updates and manages
                    // situations where either the rendering or the updating is slow.
                    // Source: http://gafferongames.com/game-physics/fix-your-timestep/
                    float gameTime = 0.0f;

                    float oldTime = (float)stopwatch.Elapsed.TotalSeconds;
                    float timeAccumulator = 0.0f;

                    while (!gameGraphics.Window.WasClosed && !gameStateManager.IsEmpty)
                    {
                        float newTime = (float)stopwatch.Elapsed.TotalSeconds;
                        float actualTimeDelta = newTime - oldTime;
                        if (actualTimeDelta > 0.2f) actualTimeDelta = 0.2f; // Helps when we break for a while during debugging
                        timeAccumulator += actualTimeDelta * TimeSpeedMultiplier;
                        oldTime = newTime;

                        while (timeAccumulator >= TargetSecondsPerFrame)
                        {
                            gameStateManager.Update(TargetSecondsPerFrame);
                            updateRateCounter.Update();

                            gameTime += TargetSecondsPerFrame;
                            timeAccumulator -= TargetSecondsPerFrame;
                        }

                        gameGraphics.Window.Update();
                        if (gameStateManager.ActiveState == null) continue;

                        gameGraphics.Context.Clear(Colors.Black);

                        gameStateManager.Draw(gameGraphics);
                        gameGraphics.Context.Present();

                        drawRateCounter.Update();
                    }
                }
            }

            Debug.Assert(Texture.AliveCount == 0,
                "Congratulations! You've leaked {0} textures!"
                .FormatInvariant(Texture.AliveCount));
        }
        #endregion
        #endregion
    }
}
