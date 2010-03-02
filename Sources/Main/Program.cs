using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Orion.Engine.Graphics;
using Orion.Engine.Networking;
using Orion.GameLogic;
using Orion.Matchmaking;
using Orion.Networking;
using Orion.UserInterface;
using Orion.Audio;
using System.Threading;
using Button = Orion.UserInterface.Widgets.Button;

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

        private GameUI gameUI;
        private SafeTransporter transporter;
        private readonly StringBuilder windowTitleStringBuilder = new StringBuilder();
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
            MatchUI matchUI = new MatchUI(gameUI.GraphicsContext, match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, faction) => matchUI.DisplayVictoryMessage(faction);

            gameUI.Display(matchUI);
            match.Start();
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

            MainMenuUI menuUI = new MainMenuUI();
            menuUI.SinglePlayerSelected += ConfigureSinglePlayerGame;
            menuUI.MultiplayerSelected += EnterMultiplayerLobby;
            menuUI.ViewReplaySelected += EnterReplayViewer;
            gameUI = new GameUI();
            gameUI.Display(menuUI);
        }

        private void StartGame(MatchConfigurer configurer)
        {
            gameUI.RootView.PopDisplay(configurer.UserInterface);

            Match match;
            SlaveCommander localCommander;
            configurer.Start(out match, out localCommander);
            MatchUI matchUI = new MatchUI(gameUI.GraphicsContext, match, localCommander);

            match.FactionMessageReceived += (sender, message) => matchUI.DisplayMessage(message);
            match.World.FactionDefeated += (sender, faction) => matchUI.DisplayDefeatMessage(faction);
            match.WorldConquered += (sender, factions) => matchUI.DisplayVictoryMessage(factions);

            gameUI.Display(matchUI);
            match.Start();

            Unit pyramid = localCommander.Faction.Units
                .First(unit => unit.Type == match.World.UnitTypes.FromName("Pyramide"));
            matchUI.CenterOn(pyramid.Center);
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
                bool windowTitleNeedsUpdate = false;

                float newTime = (float)stopwatch.Elapsed.TotalSeconds;
                float actualTimeDelta = newTime - oldTime;
                if (actualTimeDelta > 0.2f) actualTimeDelta = 0.2f; // Helps when we break for a while during debugging
                timeAccumulator += actualTimeDelta * TimeSpeedMultiplier;
                oldTime = newTime;

                while (timeAccumulator >= TargetSecondsPerFrame)
                {
                    gameUI.Update(TargetSecondsPerFrame);
                    windowTitleNeedsUpdate |= updateRateCounter.Update();

                    gameTime += TargetSecondsPerFrame;
                    timeAccumulator -= TargetSecondsPerFrame;
                }

                Application.DoEvents();
                gameUI.Refresh();
                windowTitleNeedsUpdate |= drawRateCounter.Update();

                if (windowTitleNeedsUpdate)
                    UpdateWindowTitle(updateRateCounter, drawRateCounter);
            }
        }

        private void UpdateWindowTitle(FrameRateCounter updateRateCounter, FrameRateCounter drawRateCounter)
        {
#if DEBUG
            windowTitleStringBuilder.Remove(0, windowTitleStringBuilder.Length);
            windowTitleStringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                "MS/U avg: {0:F2}, peak: {1:F2}; MS/D avg: {2:F2}, peak: {3:F2}",
                updateRateCounter.AverageMillisecondsPerFrame,
                updateRateCounter.PeakMillisecondsPerFrame,
                drawRateCounter.AverageMillisecondsPerFrame,
                drawRateCounter.PeakMillisecondsPerFrame);
            gameUI.WindowTitle = windowTitleStringBuilder.ToString();
#endif
        }
        #endregion

        #region Object Model
        public void Dispose()
        {
            gameUI.Dispose();
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

            using (Program program = new Program())
            {
                program.StartProgram();
                program.Run();
            }

            Debug.Assert(Texture.AliveCount == 0,
                "Congratulations! You've leaked {0} textures!"
                .FormatInvariant(Texture.AliveCount));
        }
        #endregion
        #endregion
    }
}