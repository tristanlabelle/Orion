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
    static class Program
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

			switch(gameType)
			{
				case MatchStartType.Solo: RunSinglePlayerGame(); return;
				case MatchStartType.Host: HostGame(); return;
				case MatchStartType.Join: JoinGame(enteredAddress); return;
			}
        }

		private static void HostGame()
		{
			using(Transporter transporter = new Transporter(41223))
			{
				List<IPEndPoint> peers;
				using(NetworkSetupHost host = new NetworkSetupHost(transporter))
				{
					host.WaitForPeers();
					peers = host.Peers;
				}
				
				RunMultiplayerGame(transporter, peers, true);
			}
		}
		
		private static void JoinGame(IPAddress host)
		{
			using(Transporter transporter = new Transporter(41223))
			{
				List<IPEndPoint> peers;
				using(NetworkSetupClient client = new NetworkSetupClient(transporter))
				{
					client.Join(new IPEndPoint(host, transporter.Port));
					client.WaitForPeers();
					peers = client.Peers;
				}
				
				RunMultiplayerGame(transporter, peers, false);
			}
		}
		
		private static void RunMultiplayerGame(Transporter transporter, List<IPEndPoint> peers, bool isAdmin)
		{
			
		}
		
		private static void RunSinglePlayerGame()
		{
            var random = new MersenneTwister();
            Console.WriteLine("Mersenne twister seed: {0}", random.Seed);
            
            Terrain terrain = Terrain.Generate(100, 100, random);
            World world = new World(terrain);

            #region Putting little guys to life
			
			CommandExecutor executor = new CommandExecutor();
			CommandLogger logger = new CommandLogger(executor);

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction, logger);

            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, executor, random);
			
			List<Commander> commanders = new List<Commander>();
			commanders.Add(redCommander);
			commanders.Add(blueCommander);

            UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
            for (int i = 0; i < 200; ++i)
            {
                Faction faction = (i % 2 == 0) ? redFaction : blueFaction;
                Unit unit = faction.CreateUnit(unitTypes[i % unitTypes.Length]);
                unit.Position = new Vector2(random.Next(world.Width), random.Next(world.Height));
            }

			#region Resource Nodes
            for (int i = 0; i < 10; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Alladium : ResourceType.Allagene;
                ResourceNode node = new ResourceNode(i, resourceType, 500, position, world);

                world.ResourceNodes.Add(node);
            }
			#endregion
            #endregion
			
            using (GameUI ui = new GameUI(world, redCommander))
            {
                const float targetFramesPerSecond = 30;
                const float targetSecondsPerFrame = 1.0f / targetFramesPerSecond;

                Stopwatch stopwatch = Stopwatch.StartNew();
                while (ui.IsWindowCreated)
                {
                    ui.Render();

                    float timeDeltaInSeconds = (float)stopwatch.Elapsed.TotalSeconds;
                    if (timeDeltaInSeconds >= targetSecondsPerFrame)
                    {
                        stopwatch.Stop();
                        stopwatch.Reset();
                        stopwatch.Start();

                        do
                        {
							foreach(Commander commander in commanders) commander.Update(targetFramesPerSecond);
                            world.Update(targetSecondsPerFrame);
                            ui.Update(targetSecondsPerFrame);

                            timeDeltaInSeconds -= targetSecondsPerFrame;
                        } while (timeDeltaInSeconds >= targetSecondsPerFrame);
                    }
                }
	        }
		}
    }
}
