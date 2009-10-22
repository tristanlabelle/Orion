using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Threading;

using OpenTK.Math;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;
using Orion.Networking;

using Color = System.Drawing.Color;

namespace Orion.Main
{
    internal static class Program
    {
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
            using (Transporter transporter = new Transporter(41223))
            {
                List<IPEndPoint> peers;
                using (NetworkSetupHost host = new NetworkSetupHost(transporter))
                {
                    host.WaitForPeers();
                    peers = host.Peers.ToList();
                }

                RunMultiplayerGame(transporter, peers, null);
            }
        }

        private static void JoinGame(IPAddress host)
        {
            using (Transporter transporter = new Transporter(41223))
            {
                List<IPEndPoint> peers;
                IPEndPoint admin = new IPEndPoint(host, transporter.Port);
                using (NetworkSetupClient client = new NetworkSetupClient(transporter))
                {
                    client.Join(admin);
                    client.WaitForPeers();
                    peers = client.Peers.ToList();
                }

                RunMultiplayerGame(transporter, peers, admin);
            }
        }

        private static void RunMultiplayerGame(Transporter transporter, IEnumerable<IPEndPoint> peers, IPEndPoint admin)
        {

        }

        private static void RunSinglePlayerGame()
        {
			MatchConfigurer configurer = new SinglePlayerMatchConfigurer();
            Run(configurer.Start());
        }

        private static void Run(Match match)
        {
            using (GameUI ui = new GameUI(match.World, match.UserCommander))
            {
                MatchRunLoop runLoop = new MatchRunLoop(ui, match.World, match);
				while(ui.IsWindowCreated) runLoop.RunOnce();
            }
        }
    }
}
