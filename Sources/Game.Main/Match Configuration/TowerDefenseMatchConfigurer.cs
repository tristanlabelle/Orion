﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Presentation;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

namespace Orion.Main
{
    sealed class TowerDefenseMatchConfigurer : MatchConfigurer
    {
        #region Constructors
        public TowerDefenseMatchConfigurer()
        {
            options.Seed = (int)Environment.TickCount;
            options.InitialAladdiumAmount = 500;
            options.InitialAlageneAmount = 0;
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
            world = new World(terrain, random, options.MaximumPopulation);
            CreepPath creepPath = CreepPath.Generate(world.Size, new Random());

            Faction localFaction = world.CreateFaction("Player", Colors.Red, options.InitialAladdiumAmount, options.InitialAlageneAmount);
            localFaction.LocalFogOfWar.Disable();
            localFaction.CreateUnit(world.UnitTypes.FromName("Métaschtroumpf"), new Point(world.Width / 2, world.Height / 2));
            localCommander = new SlaveCommander(localFaction);
            
            Faction creepFaction = world.CreateFaction("Creeps", Colors.Cyan, 0, 0);
            Commander creepCommander = new CreepWaveCommander(creepFaction, creepPath);

            world.Entities.Removed += (sender, entity) =>
                {
                    Unit unit = entity as Unit;
                    bool isKilledCreep = unit != null
                        && unit.Faction == creepFaction
                        && !unit.GridRegion.Contains(creepPath.Points[creepPath.Points.Count - 1]);

                    if (!isKilledCreep) return;

                    localFaction.AladdiumAmount += (int)(unit.GetStat(BasicSkill.AladdiumCostStat) * 0.1f);
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
