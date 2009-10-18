using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

using OpenTK.Math;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

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

            using (MatchSettingsDialog dialog = new MatchSettingsDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                if (dialog.StartType != MatchStartType.Solo)
                {
                    MessageBox.Show("Match type not implemented.", "Error!");
                    return;
                }
            }

            var random = new MersenneTwister();
            Console.WriteLine("Mersenne twister seed: {0}", random.Seed);

            Terrain terrain = TerrainGenerator.GenerateNewTerrain(100, 100, random);
            World world = new World(terrain);

            #region Putting little guys to life

            CommandManager commandManager = new CommandManager(world);

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);
            commandManager.AddCommander(redCommander);

            Faction blueFaction = world.CreateFaction("Blue", Color.Blue);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, random);
            commandManager.AddCommander(blueCommander);

            UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
            for (int i = 0; i < 200; ++i)
            {
                Faction faction = (i % 2 == 0) ? redFaction : blueFaction;
                Unit unit = faction.CreateUnit(unitTypes[i % unitTypes.Length]);
                unit.Position = new Vector2(random.Next(world.Width), random.Next(world.Height));
            }

            //Adding Resource Nodes to the game world
            for (int i = 0; i < 10; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Alladium : ResourceType.Allagene;
                ResourceNode node = new ResourceNode(i, resourceType, 500, position, world);

                world.ResourceNodes.Add(node);
            }
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
                            commandManager.Update(targetSecondsPerFrame);
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
