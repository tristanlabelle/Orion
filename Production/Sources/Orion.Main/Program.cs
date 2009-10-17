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

            Random random = new MersenneTwister();
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

            //Adding Ressource Nodes to the game world
            for (int i = 0; i < 5; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                RessourceNode node = new RessourceNode(i, RessourceType.Alladium, 500, position, world);

                world.RessourceNodes.Add(node);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                RessourceNode node = new RessourceNode(i, RessourceType.Allagene, 500, position, world);

                world.RessourceNodes.Add(node);
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
