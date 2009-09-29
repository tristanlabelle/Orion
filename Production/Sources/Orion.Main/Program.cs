using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Color = System.Drawing.Color;

using OpenTK.Math;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
    static class Program
    {
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        static void Main()
        {
            World world = new World();

            // putting little guys to life
            {
                CommandManager commandManager = new CommandManager();

                Faction redFaction = new Faction(world, "Red", Color.Red);
                MockCommander redCommander = new MockCommander(redFaction);
                commandManager.AddCommander(redCommander);

                Faction blueFaction = new Faction(world, "Blue", Color.Blue);
                MockCommander blueCommander = new MockCommander(blueFaction);
                commandManager.AddCommander(blueCommander);

                UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
                Random random = new Random();
                for (int i = 0; i < 60; ++i)
                {
                    Unit unit = new Unit((uint)i, unitTypes[i % unitTypes.Length], world);
                    unit.Position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                    unit.Faction = (i % 2) == 0 ? redFaction : blueFaction;
                    world.Units.Add(unit);
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GameUI renderer = new GameUI(world);
            Application.Run(renderer.MainWindow);
        }
    }
}
