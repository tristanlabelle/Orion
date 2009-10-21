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
            Run(new SinglePlayerCommandPipeline());
        }

        private static void Run(CommandPipeline pipeline)
        {
            var random = new MersenneTwister();
            Console.WriteLine("Mersenne twister seed: {0}", random.Seed);

            Terrain terrain = Terrain.Generate(128, 128, random);
            World world = new World(terrain);

            #region Putting little guys to life

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);

            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, random);

            redCommander.AddToPipeline(pipeline);
            blueCommander.AddToPipeline(pipeline);

            UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
            for (int i = 0; i < 200; ++i)
            {
                Faction faction = (i % 2 == 0) ? redFaction : blueFaction;
                Unit unit = faction.CreateUnit(unitTypes[i % unitTypes.Length]);
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y))
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                }
                unit.Position = position;
            }

            #region Resource Nodes
            for (int i = 0; i < 10; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y))
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                }
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Alladium : ResourceType.Alagene;
                ResourceNode node = new ResourceNode(i, resourceType, 500, position, world);

                world.ResourceNodes.Add(node);
            }
            #endregion

            #region Buildings
            Vector2 buildingPosition = new Vector2(random.Next(world.Width), random.Next(world.Height));
            while (!world.Terrain.IsWalkable((int)buildingPosition.X, (int)buildingPosition.Y))
            {
                buildingPosition = new Vector2(random.Next(world.Width), random.Next(world.Height));
            }
            Unit building = redFaction.CreateUnit(new UnitType("building"));
            building.Position = buildingPosition;
            #endregion
            #endregion

            using (GameUI ui = new GameUI(world, redCommander))
            {
                MatchRunLoop runLoop = new MatchRunLoop(ui, world, pipeline);
				while(ui.IsWindowCreated) runLoop.RunOnce();
            }
        }
    }
}
