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
            MockCommander commander = new MockCommander(faction);
            commandManager.AddCommander(commander);

            commandManager.QueryCommands();
            commandManager.ExecuteCommandQueue();


            Console.ReadLine();
        }
    }
}