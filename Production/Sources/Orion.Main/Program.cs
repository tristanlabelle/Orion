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

            #region putting little guys to life

            CommandManager commandManager = new CommandManager();

            Faction redFaction = new Faction(world, "Red", Color.Red);
            MockCommander redCommander = new MockCommander(redFaction);
            commandManager.AddCommander(redCommander);

            Faction blueFaction = new Faction(world, "Blue", Color.Blue);
            MockCommander blueCommander = new MockCommander(blueFaction);
            commandManager.AddCommander(blueCommander);

            UnitType[] unitTypes = new[] { new UnitType("Archer"), new UnitType("Tank"), new UnitType("Jedi") };
            Random random = new Random();
            for (int i = 0; i < 600; ++i)
            {
                Unit unit = new Unit((uint)i, unitTypes[i % unitTypes.Length], world);
                unit.Position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                unit.Faction = (i % 2) == 0 ? redFaction : blueFaction;
                world.Units.Add(unit);
            }
            #endregion

            using (GameUI ui = new GameUI(world))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (ui.IsWindowCreated)
                {
                    ui.Render();
                    float timeDelta = (float)stopwatch.Elapsed.TotalSeconds;
                    commandManager.Update(timeDelta);
                    world.Update(timeDelta);
                    stopwatch.Reset();
                    stopwatch.Start();
                }
            }
        }

    }
}
