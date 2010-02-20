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

            Faction blueFaction = new Faction(world, "Blue", Color.Blue);
            MockCommander blueCommander = new MockCommander(blueFaction);
            commandManager.AddCommander(blueCommander);

            UnitType jedi = new UnitType("Jedi");
            UnitType pirate = new UnitType("Pirate");
            UnitType ninja = new UnitType("Ninja");

            redFaction.CreateUnit(jedi);
            redFaction.CreateUnit(pirate);
            redFaction.CreateUnit(ninja);

            blueFaction.CreateUnit(jedi);
            blueFaction.CreateUnit(pirate);
            blueFaction.CreateUnit(ninja);

            commandManager.Update(0.5f);
            Console.ReadLine();
        }
    }
}