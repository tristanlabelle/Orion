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

            World world = new World();

            #region Putting little guys to life

            CommandManager commandManager = new CommandManager();

            Faction redFaction = new Faction(world, "Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);
            commandManager.AddCommander(redCommander);

            Faction blueFaction = new Faction(world, "Blue", Color.Blue);
            MockCommander blueCommander = new MockCommander(blueFaction);
            //commandManager.AddCommander(blueCommander);

            UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
            Random random = new Random();
            for (int i = 0; i < 1600; ++i)
            {
                Faction faction = (i % 2 == 0) ? redFaction : blueFaction;
                Unit unit = faction.CreateUnit(unitTypes[i % unitTypes.Length]);
                unit.Position = new Vector2(random.Next(world.Width), random.Next(world.Height));
            }

            //Adding Ressource Nodes to the game world
            for (int i = 0; i < 5; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                RessourceNode node = new RessourceNode(i, RessourceType.Alladium, 500, position);

                world.RessourceNodes.Add(node);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                RessourceNode node = new RessourceNode(i, RessourceType.Allagene, 500, position);

                world.RessourceNodes.Add(node);
            }


            #endregion

            using (GameUI ui = new GameUI(world, redCommander))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (ui.IsWindowCreated)
                {
                    ui.Render();

                    float timeDelta = (float)stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Stop();
                    stopwatch.Reset();
                    stopwatch.Start();

                    commandManager.Update(timeDelta);
                    world.Update(timeDelta);
                    ui.Update(timeDelta);
                }
            }
        }

    }
}
