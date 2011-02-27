using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using System.Diagnostics;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A delegate to a method which applies the effect of a cheat code.
    /// </summary>
    /// <param name="match">The match in which the cheat code is applied.</param>
    /// <param name="faction">The faction which used the cheat code.</param>
    public delegate void CheatCodeEffect(Match match, Faction faction);

    /// <summary>
    /// Holds a collection of cheat codes.
    /// </summary>
    public sealed class CheatCodeManager
    {
        #region Instance
        #region Fields
        private readonly Dictionary<string, CheatCodeEffect> codes
            = new Dictionary<string, CheatCodeEffect>();
        #endregion

        #region Properties
        public IEnumerable<string> Names
        {
            get { return codes.Keys; }
        }
        #endregion

        #region Methods
        public void Register(string name, CheatCodeEffect effect)
        {
            Argument.EnsureNotNullNorEmpty(name, "name");
            Argument.EnsureNotNull(effect, "effect");

            codes.Add(name, effect);
        }

        public bool Exists(string name)
        {
            return codes.ContainsKey(name);
        }

        public void Execute(string name, Match match, Faction faction)
        {
            Argument.EnsureNotNullNorEmpty(name, "name");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(faction, "faction");
            codes[name](match, faction);
        }
        #endregion
        #endregion

        #region Static
        #region Fields
        /// <summary>
        /// A globally accessible instance with the default cheat codes.
        /// </summary>
        public static readonly CheatCodeManager Default;
        #endregion

        #region Constructor
        static CheatCodeManager()
        {
            Default = new CheatCodeManager();
            Default.Register("colorlessdeepfog", DisableFogOfWar);
            Default.Register("ferdinandmagellan", RevealMap);
            Default.Register("magiclamp", IncreaseResources);
            Default.Register("twelvehungrymen", IncreaseAvailableFood);
            Default.Register("whosyourdaddy", SpawnChuckNorris);
            Default.Register("brinformatique", InstantDefeat);
            Default.Register("turboturbo", IncreaseBuildAndTrainSpeed);
            Default.Register("itsover9000", InstantVictory);
            Default.Register("catchmymohawk", SpawnMisterT);
#if DEBUG
            Default.Register("!", MasterCheat);
#endif
        }
        #endregion

        #region Methods
        private static void DisableFogOfWar(Match match, Faction faction)
        {
            faction.LocalFogOfWar.Disable();
        }

        private static void RevealMap(Match match, Faction faction)
        {
            faction.LocalFogOfWar.Reveal();
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

        private static void SpawnChuckNorris(Match match, Faction faction)
        {
            Unit unitType = match.UnitTypes.FromName("ChuckNorris");
            if (unitType == null)
            {
                Debug.Fail("Failed to find hero unit type.");
                return;
            }

            faction.CreateUnit(unitType, (Point)match.World.Bounds.Center);
        }

        private static void InstantVictory(Match match, Faction faction)
        {
            List<Entity> enemyUnits = match.World.Entities
                .Where(e => FactionMembership.GetFaction(e) != faction)
                .ToList();
            foreach (Entity enemy in enemyUnits)
            {
                Health health = enemy.Components.TryGet<Health>();
                if (health != null) health.Value = 0;
            }
        }

        private static void IncreaseBuildAndTrainSpeed(Match match, Faction faction)
        {
            Technology technology = match.TechnologyTree.Technologies
                .FirstOrDefault(tech => tech.Name == "TurboTurbo");
            if (technology == null)
            {
                Debug.Fail("Failed to find fast build & train technology.");
                return;
            }

            faction.AddResearchedTechnology(technology);
        }

        private static void InstantDefeat(Match match, Faction faction)
        {
            List<Unit> userBuildings = match.World.Entities
                .OfType<Unit>().Where(u => u.Faction == faction).ToList();
            foreach (Unit building in userBuildings) building.Suicide();
        }

        private static void SpawnMisterT(Match match, Faction faction)
        {
            Unit heroUnitType = match.UnitTypes.FromName("MrT");
            faction.CreateUnit(heroUnitType, (Point)match.World.Bounds.Center);
        }

        private static void MasterCheat(Match match, Faction faction)
        {
            DisableFogOfWar(match, faction);
            IncreaseResources(match, faction);
            IncreaseAvailableFood(match, faction);
            IncreaseBuildAndTrainSpeed(match, faction);
        }
        #endregion
        #endregion
    }
}
