using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Main
{
    internal class Program : IDisposable
    {
        #region Fields
        private const float TargetFramesPerSecond = 40;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const float TimeSpeedMultiplier = 1;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;

        private GameGraphics gameGraphics;
        private SafeTransporter transporter;
        private readonly StringBuilder windowTitleStringBuilder = new StringBuilder();
        #endregion

        #region Methods
        #region Main Menu Event Handlers
        private void ConfigureSinglePlayerGame(MainMenuUI sender)
        {
            MatchConfigurer configurer = new SinglePlayerMatchConfigurer();
            configurer.GameStarted += StartGame;
            gameGraphics.RootView.PushDisplay(configurer.UserInterface);
        }

        private void EnterMultiplayerLobby(MainMenuUI sender)
        {
            HostMultiplayerLobby lobby = new HostMultiplayerLobby(transporter);
            lobby.HostedGame += BeginHostMultiplayerGame;
            lobby.JoinedGame += JoinedMultiplayerGame;
            gameGraphics.RootView.PushDisplay(lobby);
        }

        private void StartTowerDefenseGame(MainMenuUI sender)
        {
            MatchConfigurer configurer = new TowerDefenseMatchConfigurer();
            StartGame(configurer);
        }

        private void EnterReplayViewer(MainMenuUI sender)
        {
            ReplayLoadingUI replayLoader = new ReplayLoadingUI();
            replayLoader.PressedStartReplay += ViewReplay;
            gameGraphics.RootView.PushDisplay(replayLoader);
        }
        #endregion

        #region Viewing Replays
        private void ViewReplay(ReplayLoadingUI ui, string fileName)
        {
            MatchConfigurer replayConfigurer = new ReplayMatchConfigurer("Replays/" + fileName);
            Match match;
            SlaveCommander localCommander;
            replayConfigurer.Start(out match, out localCommander);
            MatchUI matchUI = new MatchUI(gameGraphics, match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, faction) => matchUI.DisplayVictoryMessage(faction);

            gameGraphics.RootView.PushDisplay(matchUI);
            match.Start();
        }
        #endregion

        #region Multiplayer Lobby Event Handlers
        private void BeginHostMultiplayerGame(HostMultiplayerLobby sender)
        {
            MultiplayerHostMatchConfigurer configurer = new MultiplayerHostMatchConfigurer(transporter);
            configurer.GameStarted += StartGame;
            gameGraphics.RootView.PushDisplay(configurer.UserInterface);
        }

        private void JoinedMultiplayerGame(HostMultiplayerLobby lobby, IPv4EndPoint host)
        {
            MultiplayerClientMatchConfigurer configurer = new MultiplayerClientMatchConfigurer(transporter, host);
            configurer.GameStarted += StartGame;
            gameGraphics.RootView.PushDisplay(configurer.UserInterface);
        }
        #endregion

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

        #region Running the Game
        private void StartProgram()
        {
            int port = DefaultHostPort;
            do
            {
                try
                {
                    transporter = new SafeTransporter(port);
                    Debug.WriteLine("Listening on port {0}.".FormatInvariant(transporter.Port));
                    break;
                }
                catch
                {
                    port++;
                }
            } while (true);

            EnableLogging();

            gameGraphics = new GameGraphics();

            MainMenuUI menuUI = new MainMenuUI(gameGraphics);
            menuUI.SinglePlayerSelected += ConfigureSinglePlayerGame;
            menuUI.MultiplayerSelected += EnterMultiplayerLobby;
            menuUI.TowerDefenseSelected += StartTowerDefenseGame;
            menuUI.ViewReplaySelected += EnterReplayViewer;

            gameGraphics.RootView.PushDisplay(menuUI);
        }

        private void StartGame(MatchConfigurer configurer)
        {
            MatchConfigurationUI matchConfigurationUI = configurer.UserInterface;
            if (matchConfigurationUI != null)
                gameGraphics.RootView.PopDisplay(matchConfigurationUI);

            Match match;
            SlaveCommander localCommander;
            configurer.Start(out match, out localCommander);
            MatchUI matchUI = new MatchUI(gameGraphics, match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, factions) => matchUI.DisplayVictoryMessage(factions);

            gameGraphics.RootView.PushDisplay(matchUI);
            match.Start();

            Unit viewTarget = localCommander.Faction.Units.FirstOrDefault(unit => unit.HasSkill<TrainSkill>())
                ?? localCommander.Faction.Units.FirstOrDefault();
            if (viewTarget != null) matchUI.CenterOn(viewTarget.Center);
        }

        private void Run()
        {

        }

        private void UpdateWindowTitle(FrameRateCounter updateRateCounter, FrameRateCounter drawRateCounter)
        {
            windowTitleStringBuilder.Remove(0, windowTitleStringBuilder.Length);
            windowTitleStringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                "Orion - MS/U avg: {0:F2}, peak: {1:F2}; MS/D avg: {2:F2}, peak: {3:F2}",
                updateRateCounter.AverageMillisecondsPerFrame,
                updateRateCounter.PeakMillisecondsPerFrame,
                drawRateCounter.AverageMillisecondsPerFrame,
                drawRateCounter.PeakMillisecondsPerFrame);
            gameGraphics.Window.Title = windowTitleStringBuilder.ToString();
        }
        #endregion

        #region Object Model
        public void Dispose()
        {
            gameGraphics.Dispose();
            transporter.Dispose();
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

            //using (Program program = new Program())
            //{
            //    program.StartProgram();
            //    program.Run();
            //}

            using (GameGraphics gameGraphics = new GameGraphics())
            {
                GameStateManager gameStateManager = new GameStateManager();
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

                while (!gameGraphics.Window.WasClosed && gameStateManager.ActiveState != null)
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

            Debug.Assert(Texture.AliveCount == 0,
                "Congratulations! You've leaked {0} textures!"
                .FormatInvariant(Texture.AliveCount));
        }
        #endregion
        #endregion
    }
}
