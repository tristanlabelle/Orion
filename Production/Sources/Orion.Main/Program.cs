using System;
using System.Net;
using System.Windows.Forms;
using Orion.Networking;
using Orion.UserInterface;


namespace Orion.Main
{
    internal static class Program
    {
        public const int DefaultPort = 41223;
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
            using (Transporter transporter = new Transporter(DefaultPort))
            {
                MultiplayerHostMatchConfigurer configurer = new MultiplayerHostMatchConfigurer(transporter);
                RunMultiplayerGame(configurer);
            }
        }

        private static void JoinGame(IPAddress host)
        {
            int port = DefaultPort;
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
            configurer.Host = host;
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
                MatchUI matchUI = new MatchUI(match.World, match.UserCommander);
                MatchRunLoop runLoop = new MatchRunLoop(ui, match.World, match);
                match.World.Entities.Update(0);
                ui.Display(matchUI);
                while (ui.IsWindowCreated) runLoop.RunOnce();
            }
        }
    }
}
