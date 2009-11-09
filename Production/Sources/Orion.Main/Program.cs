using System;
using System.Net;
using System.Windows.Forms;
using Orion.Networking;
using Orion.UserInterface;
using System.Diagnostics;


namespace Orion.Main
{
    internal static class Program
    {
        private const float TargetFramesPerSecond = 60;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;
        private const int MaxSuccessiveUpdates = 5;

        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MatchStartType gameType;
            IPAddress enteredAddress;
            using (MatchSettingsDialog dialog = new MatchSettingsDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                gameType = dialog.StartType;
                enteredAddress = dialog.Host;
            }

            switch (gameType)
            {
                case MatchStartType.Solo: RunSinglePlayerGame(); return;
                case MatchStartType.Host: HostGame(); return;
                case MatchStartType.Join: JoinGame(enteredAddress); return;
            }
        }

        private static void HostGame()
        {
            using (Transporter transporter = new Transporter(DefaultHostPort))
            {
                MultiplayerHostMatchConfigurer configurer = new MultiplayerHostMatchConfigurer(transporter);
                RunMultiplayerGame(configurer);
            }
        }

        private static void JoinGame(IPAddress hostAddress)
        {
            int port = DefaultClientPort;
            Transporter transporter;

            do
            {
                try
                {
                    transporter = new Transporter(port);
                    break;
                }
                catch { port++; }
            } while (true);

            MultiplayerClientMatchConfigurer configurer = new MultiplayerClientMatchConfigurer(transporter);
            configurer.Host = new IPEndPoint(hostAddress, DefaultHostPort);
            RunMultiplayerGame(configurer);

            transporter.Dispose();
        }

        private static void RunMultiplayerGame(MultiplayerMatchConfigurer configurer)
        {
            configurer.CreateNetworkConfiguration();
            Start(configurer);
        }

        private static void RunSinglePlayerGame()
        {
            MatchConfigurer configurer = new SinglePlayerMatchConfigurer();
            Start(configurer);
        }

        private static void Start(MatchConfigurer configurer)
        {
            configurer.NumberOfPlayers = 2;

            Console.WriteLine("Mersenne Twister Seed: {0}", configurer.Seed);
            Match match = configurer.Start();
            // todo: use a root menu UI instead of the match display here
            using (GameUI ui = new GameUI())
            {
                RunMatch(match, ui);
            }
        }

        private static void RunMatch(Match match, GameUI ui)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(ui, "ui");

            MatchUI matchUI = new MatchUI(match.World, match.UserCommander);
            ui.Display(matchUI);

            Stopwatch stopwatch = Stopwatch.StartNew();
            FrameRateCounter updateRateCounter = new FrameRateCounter();
            FrameRateCounter drawRateCounter = new FrameRateCounter();
            while (ui.IsWindowCreated)
            {
                Application.DoEvents();
                ui.Refresh();
                drawRateCounter.Update();

                float timeDeltaInSeconds = (float)stopwatch.Elapsed.TotalSeconds;
                if (timeDeltaInSeconds >= TargetSecondsPerFrame)
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    int successiveUpdateCount = 0;
                    do
                    {
                        match.Update(TargetSecondsPerFrame);

                        ui.Update(TargetSecondsPerFrame);
                        updateRateCounter.Update();
                        ui.WindowTitle = "{0:F2} updates, {1:F2} draws per second"
                            .FormatInvariant(updateRateCounter.FramesPerSecond, drawRateCounter.FramesPerSecond);

                        timeDeltaInSeconds -= TargetSecondsPerFrame;
                        ++successiveUpdateCount;
                    } while (timeDeltaInSeconds >= TargetSecondsPerFrame
                        && successiveUpdateCount < MaxSuccessiveUpdates);
                }
            }
        }
    }
}
