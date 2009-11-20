using System;
using System.Net;
using System.Windows.Forms;
using Orion.Networking;
using Orion.UserInterface;
using System.Diagnostics;
using Button = Orion.UserInterface.Widgets.Button;

namespace Orion.Main
{
    internal class Program : IDisposable
    {
        private const float TargetFramesPerSecond = 40;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;
        private const int MaxSuccessiveUpdates = 5;

        private GameUI gameUi;
        private SafeTransporter transporter;

        private void StartProgram()
        {
            int port = DefaultHostPort;
            do
            {
                try
                {
                    transporter = new SafeTransporter(port);
                    Console.WriteLine("Listening on port {0}", transporter.Port);
                    break;
                }
                catch
                {
                    port++;
                }
            } while (true);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainMenuUI menuUI = new MainMenuUI(ConfigureSinglePlayerGame, EnterMultiplayerLobby);
            gameUi = new GameUI();
            gameUi.Display(menuUI);
        }

        private void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            FrameRateCounter updateRateCounter = new FrameRateCounter();
            FrameRateCounter drawRateCounter = new FrameRateCounter();

            while (gameUi.IsWindowCreated)
            {
                Application.DoEvents();
                gameUi.Refresh();
                drawRateCounter.Update();

                float timeDeltaInSeconds = (float)stopwatch.Elapsed.TotalSeconds;
                if (timeDeltaInSeconds >= TargetSecondsPerFrame)
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    gameUi.Update(TargetSecondsPerFrame);
                    updateRateCounter.Update();
                    gameUi.WindowTitle = "{0:F2} updates, {1:F2} draws per second"
                        .FormatInvariant(updateRateCounter.FramesPerSecond, drawRateCounter.FramesPerSecond);
                }
            }
        }

        private void ConfigureSinglePlayerGame(Button sender)
        {
            MatchConfigurer configurer = new SinglePlayerMatchConfigurer();
            configurer.GameStarted += StartGame;
            gameUi.Display(configurer.UserInterface);
        }

        private void BeginHostMultiplayerGame(LocalMultiplayerLobby sender)
        {
            MultiplayerHostMatchConfigurer configurer = new MultiplayerHostMatchConfigurer(transporter);
            configurer.GameStarted += StartGame;
            gameUi.RootView.PushDisplay(configurer.UserInterface);
        }

        private void JoinedMultiplayerGame(LocalMultiplayerLobby lobby, IPv4EndPoint host)
        {
            MultiplayerClientMatchConfigurer configurer = new MultiplayerClientMatchConfigurer(transporter, host);
            configurer.GameStarted += StartGame;
            gameUi.RootView.PushDisplay(configurer.UserInterface);
        }

        private void StartGame(MatchConfigurer configurer)
        {
            gameUi.RootView.PopDisplay(configurer.UserInterface);
            BeginMatch(configurer.Start());
        }

        private void BeginMatch(Match match)
        {
            MatchUI matchUI = new MatchUI(match.World, match.UserCommander);
            matchUI.Updated += match.Update;

            gameUi.Display(matchUI);
        }

        private void EnterMultiplayerLobby(Button sender)
        {
            LocalMultiplayerLobby lobby = new LocalMultiplayerLobby(transporter);
            lobby.HostedGame += BeginHostMultiplayerGame;
            lobby.JoinedGame += JoinedMultiplayerGame;
            gameUi.Display(lobby);
        }

        public void Dispose()
        {
            gameUi.Dispose();
            transporter.Dispose();
        }

        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (Program program = new Program())
            {
                program.StartProgram();
                program.Run();
            }
        }
    }
}
