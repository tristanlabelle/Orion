using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment.Commands;
using Orion.GameLogic;

using OpenTK.Math;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command filter which executes cheat codes if messages match them.
    /// </summary>
    public sealed class CheatCodeExecutor : CommandFilter
    {
        #region Instance
        #region Fields
        private readonly Match match;
        private readonly Queue<Command> accumulatedCommands = new Queue<Command>();
        #endregion

        #region Constructors
        public CheatCodeExecutor(Match match)
        {
            Argument.EnsureNotNull(match, "match");
            this.match = match;
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (accumulatedCommands.Count > 0)
            {
                Command command = accumulatedCommands.Dequeue();
                Message message = command as Message;
                if (message != null)
                {
                    Action<Match> cheatCode;
                    if (cheatCodes.TryGetValue(message.Text, out cheatCode))
                    {
                        cheatCode(match);
                        command = new Message(message.FactionHandle, "Cheat '{0}' enabled!".FormatInvariant(message.Text));
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
        static CheatCodeExecutor()
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
