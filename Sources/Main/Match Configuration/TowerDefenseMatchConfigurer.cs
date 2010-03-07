using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.UserInterface;
using Orion.Matchmaking;
using System.Diagnostics;
using Orion.GameLogic;
using Orion.Matchmaking.Commands.Pipeline;
using Orion.Matchmaking.TowerDefense;

namespace Orion.Main
{
    sealed class TowerDefenseMatchConfigurer : MatchConfigurer
    {
        #region Constructors
        public TowerDefenseMatchConfigurer()
        {
            Seed = (int)Environment.TickCount;
        }
        #endregion

        #region Methods
        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return null; }
        }

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            Debug.WriteLine("Mersenne Twister Seed: {0}.".FormatInvariant(Seed));
            random = new MersenneTwister(Seed);
            Terrain terrain = Terrain.CreateFullyWalkable(new Size(60, 40));
            world = new World(terrain, random);
            CreepPath creepPath = CreepPath.Generate(world.Size, new Random());

            Faction localFaction = world.CreateFaction("Player", Colors.Red);
            localFaction.LocalFogOfWar.Disable();
            localFaction.CreateUnit(world.UnitTypes.FromName("Métaschtroumpf"), new Point(world.Width / 2, world.Height / 2));
            localCommander = new SlaveCommander(localFaction);
            
            Faction aiFaction = world.CreateFaction("Creeps", Colors.Blue);
            Commander aiCommander = new CreepWaveCommander(aiFaction, creepPath);

            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.AddCommander(localCommander);
            pipeline.AddCommander(aiCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
        #endregion
    }
}
