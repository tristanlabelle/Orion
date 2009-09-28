using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;

using Color = System.Drawing.Color;

namespace Orion.Commandment
{
    class ConsoleTest
    {
        static void Main(string[] args)
        {
            World world = new World();
            CommandManager commandManager = new CommandManager();

            Faction redFaction = new Faction(world, "Red", Color.Red);
            MockCommander redCommander = new MockCommander(redFaction);
            commandManager.AddCommander(redCommander);

            Unit redJedi = new Unit(0, new UnitType("Jedi"), world);
            Unit redPirate = new Unit(1, new UnitType("Pirate"), world);
            Unit redNinja = new Unit(2, new UnitType("Ninja"), world);
            redJedi.Faction = redFaction;
            redPirate.Faction = redFaction;
            redNinja.Faction = redFaction;
            world.Units.Add(redJedi);
            world.Units.Add(redPirate);
            world.Units.Add(redNinja);

            Faction blueFaction = new Faction(world, "Blue", Color.Blue);
            MockCommander blueCommander = new MockCommander(blueFaction);
            commandManager.AddCommander(blueCommander);

            Unit blueJedi = new Unit(3, new UnitType("Jedi"), world);
            Unit bluePirate = new Unit(4, new UnitType("Pirate"), world);
            Unit blueNinja = new Unit(5, new UnitType("Ninja"), world);
            blueJedi.Faction = blueFaction;
            bluePirate.Faction = blueFaction;
            blueNinja.Faction = blueFaction;
            world.Units.Add(blueJedi);
            world.Units.Add(bluePirate);
            world.Units.Add(blueNinja);

            commandManager.Update(0.5f);
            Console.ReadLine();
        }
    }
}