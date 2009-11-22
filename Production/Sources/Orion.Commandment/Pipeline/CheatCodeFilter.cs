using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment.Commands;

namespace Orion.Commandment.Pipeline
{
    public delegate void CheatEffect(Match match);

    public class CheatCodeFilter : CommandFilter
    {
        #region Static
        private static readonly Dictionary<string, CheatEffect> cheatCodes = new Dictionary<string, CheatEffect>();

        static CheatCodeFilter()
        {
            cheatCodes["colorlessdeepfog"] = DisableFogOfWar;
            cheatCodes["magiclamp"] = IncreaseResources;
            cheatCodes["twelvehungrymen"] = IncreaseAvailableFood;
        }

        private static void DisableFogOfWar(Match match)
        {
            match.UserCommander.Faction.FogOfWar.Disable();
        }

        private static void IncreaseResources(Match match)
        {
            match.UserCommander.Faction.AladdiumAmount += 5000;
            match.UserCommander.Faction.AlageneAmount += 5000;
        }

        private static void IncreaseAvailableFood(Match match)
        {
            match.UserCommander.Faction.UsedFoodStock -= 100;
        }

        #endregion

        #region Instance
        private Match match;
        private Queue<Command> commandQueue = new Queue<Command>();

        public CheatCodeFilter(Match match)
        {
            this.match = match;
        }

        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            commandQueue.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (commandQueue.Count > 0)
            {
                Command command = commandQueue.Dequeue();
                if (command is Message)
                {
                    Message messageCommand = (Message)command;
                    if (cheatCodes.ContainsKey(messageCommand.Value))
                    {
                        cheatCodes[messageCommand.Value](match);
                        command = new Message(messageCommand.FactionHandle, "Cheat '{0}' enabled!".FormatInvariant(messageCommand.Value));
                    }
                }
                Flush(command);
            }
        }
        #endregion
    }
}
