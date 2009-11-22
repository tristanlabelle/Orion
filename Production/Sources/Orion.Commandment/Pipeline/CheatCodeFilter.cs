using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment.Commands;
using Orion.GameLogic;

using OpenTK.Math;

namespace Orion.Commandment.Pipeline
{
    public sealed class CheatCodeFilter : CommandFilter
    {
        #region Instance
        #region Fields
        private Match match;
        private Queue<Command> commandQueue = new Queue<Command>();
        #endregion

        #region Constructors
        public CheatCodeFilter(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            this.match = match;
        }
        #endregion

        #region Methods
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
                Message message = command as Message;
                if (message != null)
                {
                    Action<Match> cheatCode;
                    if (cheatCodes.TryGetValue(message.Value, out cheatCode))
                    {
                        cheatCode(match);
                        command = new Message(message.FactionHandle, "Cheat '{0}' enabled!".FormatInvariant(message.Value));
                    }
                }
                Flush(command);
            }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Dictionary<string, Action<Match>> cheatCodes
            = new Dictionary<string, Action<Match>>();
        #endregion

        #region Constructor
        static CheatCodeFilter()
        {
            cheatCodes["colorlessdeepfog"] = DisableFogOfWar;
            cheatCodes["magiclamp"] = IncreaseResources;
            cheatCodes["twelvehungrymen"] = IncreaseAvailableFood;
            cheatCodes["whosyourdaddy"] = SpawnHeroUnit;
        }
        #endregion

        #region Methods
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

        private static void SpawnHeroUnit(Match match)
        { 
            match.UserCommander.Faction.CreateUnit(match.World.UnitTypes.First(type => type.Name == "Chuck Norris"), new Vector2(match.World.Width / 2, match.World.Height / 2));
        }
        #endregion
        #endregion
    }
}
