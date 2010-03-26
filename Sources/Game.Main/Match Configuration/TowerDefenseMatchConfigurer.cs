using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Presentation;

namespace Orion.Main
{
    sealed class TowerDefenseMatchConfigurer : MatchConfigurer
    {
        #region Constructors
        public TowerDefenseMatchConfigurer()
        {
            options.Seed = (int)Environment.TickCount;
        }
        #endregion

        #region Methods
        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return null; }
        }

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(options.Seed));
            random = new MersenneTwister(options.Seed);
            Terrain terrain = Terrain.CreateFullyWalkable(new Size(60, 40));
            world = new World(terrain, random);
            CreepPath creepPath = CreepPath.Generate(world.Size, new Random());

            Faction localFaction = world.CreateFaction("Player", Colors.Red);
            localFaction.AladdiumAmount = 500;
            localFaction.AlageneAmount = 0;
            localFaction.LocalFogOfWar.Disable();
            localFaction.CreateUnit(world.UnitTypes.FromName("Métaschtroumpf"), new Point(world.Width / 2, world.Height / 2));
            localCommander = new SlaveCommander(localFaction);
            
            Faction creepFaction = world.CreateFaction("Creeps", Colors.Cyan);
            Commander creepCommander = new CreepWaveCommander(creepFaction, creepPath);

            world.Entities.Removed += (sender, entity) =>
                {
                    Unit unit = entity as Unit;
                    bool isKilledCreep = unit != null
                        && unit.Faction == creepFaction
                        && !unit.GridRegion.Contains(creepPath.Points[creepPath.Points.Count - 1]);

                    if (!isKilledCreep) return;

                    localFaction.AladdiumAmount += (int)(unit.GetStat(UnitStat.AladdiumCost) * 0.1f);
                };

            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.AddCommander(localCommander);
            pipeline.AddCommander(creepCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
        #endregion
    }
}
