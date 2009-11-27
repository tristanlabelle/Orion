using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment.Commands;
using Orion.GameLogic;
using Skills = Orion.GameLogic.Skills;

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
                SendMessage message = command as SendMessage;
                if (message != null)
                {
                    Action<Match, Faction> cheatCode;
                    if (cheatCodes.TryGetValue(message.Text, out cheatCode))
                    {
                        cheatCode(match, match.World.FindFactionFromHandle(message.FactionHandle));
                        command = new SendMessage(message.FactionHandle, "Cheat '{0}' enabled!".FormatInvariant(message.Text));
                    }
                }
                Flush(command);
            }
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        private static readonly Dictionary<string, Action<Match, Faction>> cheatCodes
            = new Dictionary<string, Action<Match, Faction>>();
        #endregion

        #region Constructor
        static CheatCodeExecutor()
        {
            cheatCodes["colorlessdeepfog"] = DisableFogOfWar;
            cheatCodes["magiclamp"] = IncreaseResources;
            cheatCodes["twelvehungrymen"] = IncreaseAvailableFood;
            cheatCodes["whosyourdaddy"] = SpawnHeroUnit;
            cheatCodes["turboturbo"] = AccelerateUnitDevelopment;
            cheatCodes["brinformatique"] = InstantDefeat;
            cheatCodes["itsover9000"] = InstantVictory;
            cheatCodes["!"] = MasterCheat;
        }
        #endregion

        #region Methods
        private static void DisableFogOfWar(Match match, Faction faction)
        {
            faction.LocalFogOfWar.Disable();
        }

        private static void IncreaseResources(Match match, Faction faction)
        {
            faction.AladdiumAmount += 5000;
            faction.AlageneAmount += 5000;
        }

        private static void IncreaseAvailableFood(Match match, Faction faction)
        {
            faction.UsedFoodAmount -= 100;
        }

        private static void SpawnHeroUnit(Match match, Faction faction)
        { 
            UnitType heroUnitType = match.World.UnitTypes.FromName("Chuck Norris");
            faction.CreateUnit(heroUnitType, (Point)match.World.Bounds.Center);
        }

        private static void AccelerateUnitDevelopment(Match match, Faction faction)
        {
            foreach (UnitType type in match.World.UnitTypes)
            {
                if (type.HasSkill<Skills.Train>()) type.GetSkill<Skills.Train>().Speed *= 50;
                if (type.HasSkill<Skills.Build>()) type.GetSkill<Skills.Build>().Speed *= 50;
            }
        }

        private static void InstantVictory(Match match, Faction faction)
        {
            IEnumerable<Unit> enemyBuildings = match.World.Entities
                .OfType<Unit>()
                .Where(u => u.Faction != faction);
            foreach (Unit building in enemyBuildings) building.Suicide();
        }

        private static void InstantDefeat(Match match, Faction faction)
        {
            IEnumerable<Unit> userBuildings = match.World.Entities
                .OfType<Unit>().Where(u => u.Faction == faction);
            foreach (Unit building in userBuildings) building.Suicide();
        }

        private static void MasterCheat(Match match, Faction faction)
        {
            DisableFogOfWar(match, faction);
            IncreaseResources(match, faction);
            IncreaseAvailableFood(match, faction);
            SpawnHeroUnit(match, faction);
            AccelerateUnitDevelopment(match, faction);
        }
        #endregion
        #endregion
    }
}
