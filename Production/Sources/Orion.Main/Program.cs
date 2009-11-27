using System;
using System.Net;
using System.Windows.Forms;
using Orion.Commandment;
using Orion.Networking;
using Orion.UserInterface;
using System.Diagnostics;
using Button = Orion.UserInterface.Widgets.Button;
using System.Collections.Generic;
using System.IO;

namespace Orion.Main
{
    internal class Program : IDisposable
    {
        #region Fields
        private const float TargetFramesPerSecond = 40;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const float TimeSpeedMultiplier = 1;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;
        private const int MaxSuccessiveUpdates = 5;

        private GameUI gameUI;
        private SafeTransporter transporter;
        #endregion

        #region Methods
        #region Main Menu Event Handlers
        private void ConfigureSinglePlayerGame(MainMenuUI sender)
        {
            MatchConfigurer configurer = new SinglePlayerMatchConfigurer();
            configurer.GameStarted += StartGame;
            gameUI.Display(configurer.UserInterface);
        }

        private void EnterMultiplayerLobby(MainMenuUI sender)
        {
            LocalMultiplayerLobby lobby = new LocalMultiplayerLobby(transporter);
            lobby.HostedGame += BeginHostMultiplayerGame;
            lobby.JoinedGame += JoinedMultiplayerGame;
            gameUI.Display(lobby);
        }

        private void EnterReplayViewer(MainMenuUI sender)
        {
            ReplayLoadingUI replayLoader = new ReplayLoadingUI();
            replayLoader.PressedStartReplay += ViewReplay;
            gameUI.Display(replayLoader);
        }
        #endregion

        #region Viewing Replays
        private void ViewReplay(ReplayLoadingUI ui, string fileName)
        {
            MatchConfigurer replayConfigurer = new ReplayMatchConfigurer("Replays/" + fileName);
            Match match;
            SlaveCommander localCommander;
            replayConfigurer.Start(out match, out localCommander);
            MatchUI matchUI = new MatchUI(match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, faction) => matchUI.DisplayVictoryMessage(faction);

            gameUI.Display(matchUI);
        }
        #endregion

        #region Multiplayer Lobby Event Handlers
        private void BeginHostMultiplayerGame(LocalMultiplayerLobby sender)
        {
            MultiplayerHostMatchConfigurer configurer = new MultiplayerHostMatchConfigurer(transporter);
            configurer.GameStarted += StartGame;
            gameUI.RootView.PushDisplay(configurer.UserInterface);
        }

        private void JoinedMultiplayerGame(LocalMultiplayerLobby lobby, IPv4EndPoint host)
        {
            MultiplayerClientMatchConfigurer configurer = new MultiplayerClientMatchConfigurer(transporter, host);
            configurer.GameStarted += StartGame;
            gameUI.RootView.PushDisplay(configurer.UserInterface);
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
                    writer.AutoFlush = true;
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

            MainMenuUI menuUI = new MainMenuUI();
            menuUI.LaunchedSinglePlayerGame += ConfigureSinglePlayerGame;
            menuUI.LaunchedMultiplayerGame += EnterMultiplayerLobby;
            menuUI.LaunchedReplayViewer += EnterReplayViewer;
            gameUI = new GameUI();
            gameUI.Display(menuUI);
        }

        private void StartGame(MatchConfigurer configurer)
        {
            gameUI.RootView.PopDisplay(configurer.UserInterface);

            Match match;
            SlaveCommander localCommander;
            configurer.Start(out match, out localCommander);
            MatchUI matchUI = new MatchUI(match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, faction) => matchUI.DisplayVictoryMessage(faction);

            gameUI.Display(matchUI);
        }

        private void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            FrameRateCounter updateRateCounter = new FrameRateCounter();
            FrameRateCounter drawRateCounter = new FrameRateCounter();

            // This run loop uses a fixed time step for the updates and manages
            // situations where either the rendering or the updating is slow.
            // Source: http://gafferongames.com/game-physics/fix-your-timestep/
            float gameTime = 0.0f;

            float oldTime = (float)stopwatch.Elapsed.TotalSeconds;
            float timeAccumulator = 0.0f;

            while (gameUI.IsWindowCreated)
            {
                float newTime = (float)stopwatch.Elapsed.TotalSeconds;
                float actualTimeDelta = newTime - oldTime;
                if (actualTimeDelta > 0.2f) actualTimeDelta = 0.2f; // Helps when we break for a while during debugging
                timeAccumulator += actualTimeDelta * TimeSpeedMultiplier;
                oldTime = newTime;

                while (timeAccumulator >= TargetSecondsPerFrame)
                {
                    gameUI.Update(TargetSecondsPerFrame);
                    updateRateCounter.Update();

                    gameTime += TargetSecondsPerFrame;
                    timeAccumulator -= TargetSecondsPerFrame;
                }

                Application.DoEvents();
                gameUI.Refresh();
                gameUI.WindowTitle = "{0:F2} ms / update, {1:F2} ms / draw"
                    .FormatInvariant(updateRateCounter.MillisecondsPerFrame, drawRateCounter.MillisecondsPerFrame);
                drawRateCounter.Update();
            }
        }
        #endregion

        #region Object Model
        public void Dispose()
        {
            gameUI.Dispose();
            transporter.Dispose();
        }
        #endregion

        #region Static
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Program program = new Program())
            {
                program.StartProgram();
                program.Run();
            }
        }
        #endregion
        #endregion
    }
}
