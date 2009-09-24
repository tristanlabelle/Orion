using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;

namespace Orion.Commandment
{
    class ConsoleTest
    {
        static void Main(string[] args)
        {

            CommandManager commandManager = new CommandManager();
            Faction faction = new Faction();
            World world = new World();

            Unit u1 = new Unit(1, new UnitType("lolwut"), world);
            u1.Faction = faction;
            Unit u2 = new Unit(2, new UnitType("lolwut"), world);
            u2.Faction = faction;
            Unit u3 = new Unit(3, new UnitType("lolwut"), world);
            u3.Faction = faction;
            Unit u4 = new Unit(4, new UnitType("lolwut"), world);
            u4.Faction = faction;

            world.Units.Add(u1);
            world.Units.Add(u2);
            world.Units.Add(u3);
            world.Units.Add(u4);


            MockCommander commander = new MockCommander(faction, world);
            commandManager.AddCommander(commander);
       
            commandManager.QueryCommands();
            commandManager.ExecuteCommandQueue();

           

            Console.ReadLine();
        }
    }
}