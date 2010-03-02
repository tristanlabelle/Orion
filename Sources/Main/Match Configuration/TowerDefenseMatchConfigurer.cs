using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.UserInterface;
using Orion.Matchmaking;
using System.Diagnostics;
using Orion.GameLogic;
using Orion.Matchmaking.Commands.Pipeline;

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

            Faction localFaction = world.CreateFaction("Player", Colors.Red);
            localFaction.LocalFogOfWar.Disable();
            localCommander = new SlaveCommander(localFaction);

            WorldGenerator.Generate(world, random);
            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            TryPushReplayRecorderToPipeline(pipeline);
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
        #endregion
    }
}
