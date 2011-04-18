using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Orion.Engine;
using Orion.Engine.Graphics;

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
            
            using (GameStateManager gameStateManager = new GameStateManager(GetAssetsPath()))
            {
                gameStateManager.Push(new MainMenuGameState(gameStateManager));

                Stopwatch stopwatch = Stopwatch.StartNew();
                FrameRateCounter updateRateCounter = new FrameRateCounter();
                FrameRateCounter drawRateCounter = new FrameRateCounter();

                // This run loop uses a fixed time step for the updates and manages
                // situations where either the rendering or the updating is slow.
                // Source: http://gafferongames.com/game-physics/fix-your-timestep/
                float gameTime = 0.0f;

                float oldTime = (float)stopwatch.Elapsed.TotalSeconds;
                float timeAccumulator = 0.0f;

                while (!gameStateManager.Graphics.Window.WasClosed && !gameStateManager.IsEmpty)
                {
                    float newTime = (float)stopwatch.Elapsed.TotalSeconds;
                    float actualTimeDelta = newTime - oldTime;
                    if (actualTimeDelta > 0.2f) actualTimeDelta = 0.2f; // Helps when we break for a while during debugging
                    timeAccumulator += actualTimeDelta * TimeSpeedMultiplier;
                    oldTime = newTime;

                    while (timeAccumulator >= TargetSecondsPerFrame)
                    {
                        gameStateManager.Update(TimeSpan.FromSeconds(TargetSecondsPerFrame));
                        updateRateCounter.Update();

                        gameTime += TargetSecondsPerFrame;
                        timeAccumulator -= TargetSecondsPerFrame;
                    }

                    gameStateManager.Graphics.Window.Update();
                    if (gameStateManager.ActiveState == null) continue;

                    gameStateManager.Graphics.Context.Clear(Colors.Black);

                    gameStateManager.Draw(gameStateManager.Graphics);
                    gameStateManager.Graphics.Context.Present();
                    
                    drawRateCounter.Update();
                }
            }

            Debug.Assert(Texture.AliveCount == 0,
                "Congratulations! You've leaked {0} textures!"
                .FormatInvariant(Texture.AliveCount));
        }
        
        private static string GetAssetsPath()
        {
            string lastAbsolutePath = null;
            string currentDirectory = Directory.GetCurrentDirectory();
            string assetsPath = "Assets";
            string absolutePath = Path.GetFullPath(Path.Combine(currentDirectory, assetsPath));
            while (!Directory.Exists(assetsPath))
            {
            	assetsPath = "../" + assetsPath;
            	lastAbsolutePath = absolutePath;
            	absolutePath = Path.GetFullPath(Path.Combine(currentDirectory, assetsPath));
            	if (absolutePath == lastAbsolutePath)
            		throw new FileNotFoundException("Could not find the assets folder!");
            }
            return assetsPath;
        }
        #endregion
        #endregion
    }
}
